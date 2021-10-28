﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure.FunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class AccountService : IAccountService
    {
        public AccountService(AetherDbContext context, IMemberContextCacheService memberContextCacheService, IFacilityService facilityService)
        {
            _context = context;
            _memberContextCacheService = memberContextCacheService;
            _facilityService = facilityService;
        }


        public Task<Result<AccountResponse>> Add(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
        {
            return Validate()
                .BindWithTransaction(_context, () => AddAccount()
                    .Bind(AddDefaultFacility)
                    .Bind(AttachToMember)
                    .Tap(ClearCache)
                    .Bind(accountId => GetAccount(accountId, cancellationToken)));


            Result Validate()
            {
                var validator = new AccountRequestValidator(context);
                var validationResult = validator.ValidateAdd(request);
                return validationResult.ToResult();
            }


            async Task<Result<int>> AddAccount()
            {
                var now = DateTime.UtcNow;

                var newAccount = new Account
                {
                    Address = request.Address,
                    Created = now,
                    Email = request.Email ?? context.Email ?? string.Empty,
                    Modified = now,
                    Name = request.Name,
                    Phone = request.Phone ?? string.Empty,
                    OperatingName = request.OperatingName ?? request.Name,
                    State = ModelStates.Active
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync(cancellationToken);

                return newAccount.Id;
            }

            async Task<Result<(int, int)>> AddDefaultFacility(int accountId)
            {
                var (_, isFailure, facilityId) = await _facilityService.AddDefault(accountId, cancellationToken);

                return isFailure 
                    ? Result.Failure<(int, int)>("Default facility hadn't been created.") 
                    : (accountId, facilityId);
            }


            async Task<Result<int>> AttachToMember((int accountId, int facilityId) tuple)
            {
                var (accountId, facilityId) = tuple;

                var member = await _context.Members
                    .SingleAsync(m => m.Id == context.Id, cancellationToken);

                member.AccountId = accountId;
                member.FacilityId = facilityId;

                _context.Members.Update(member);
                await _context.SaveChangesAsync(cancellationToken);

                return accountId;
            }


            void ClearCache()
                => _memberContextCacheService.Remove(context.IdentityHash);
        }


        public Task<Result<AccountResponse>> Get(MemberContext context, int accountId, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(() => GetAccount(accountId, cancellationToken))
                .Check(response => response.IsActive ? Result.Success() : Result.Failure("The account is switched off."));

            Result Validate()
            {
                var validator = new AccountRequestValidator(context);
                var validationResult = validator.ValidateGet(AccountRequest.CreateEmpty(accountId));
                return validationResult.ToResult();
            }
        }


        public Task<Result<AccountResponse>> Update(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(UpdateAccount)
                .Bind(() => GetAccount(request.Id!.Value, cancellationToken));


            Result Validate()
            {
                var validator = new AccountRequestValidator(context);
                var validationResult = validator.ValidateUpdate(request);
                return validationResult.ToResult();
            }


            async Task<Result> UpdateAccount()
            {
                var existingAccount = await _context.Accounts
                    .Where(a => a.Id == request.Id!.Value)
                    .SingleOrDefaultAsync(cancellationToken);

                existingAccount.Address = request.Address;
                existingAccount.Email = request.Email ?? string.Empty;
                existingAccount.Modified = DateTime.UtcNow;
                existingAccount.Name = request.Name;
                existingAccount.OperatingName = request.OperatingName ?? string.Empty;
                existingAccount.Phone = request.Phone ?? string.Empty;

                _context.Accounts.Update(existingAccount);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        private async Task<Result<AccountResponse>> GetAccount(int accountId, CancellationToken cancellationToken)
            => await _context.Accounts
                .Where(a => a.Id == accountId && a.State == ModelStates.Active)
                .Select(a => new AccountResponse(a.Id, a.Name, a.OperatingName, a.Address, a.Email, a.Phone, a.State == ModelStates.Active))
                .SingleOrDefaultAsync(cancellationToken);


        private readonly AetherDbContext _context;
        private readonly IMemberContextCacheService _memberContextCacheService;
        private readonly IFacilityService _facilityService;
    }
}
