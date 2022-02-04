using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
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
using TipCatDotNet.Api.Services.Payments;

namespace TipCatDotNet.Api.Services.HospitalityFacilities;

public class AccountService : IAccountService
{
    public AccountService(AetherDbContext context, IStripeAccountService stripeAccountService, IMemberContextCacheService memberContextCacheService, IFacilityService facilityService)
    {
        _context = context;
        _memberContextCacheService = memberContextCacheService;
        _facilityService = facilityService;
        _stripeAccountService = stripeAccountService;
    }


    public Task<Result<AccountResponse>> Add(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .BindWithTransaction(_context, () => AddAccount()
                .Bind(AddDefaultFacility)
                .Bind(AttachToMember)
                .Tap(ClearMemberContextCache)) // clear it, because the context changes after account's creation
            .Bind(accountId => Result.Success()
                .Map(() => _facilityService.Get(accountId, cancellationToken))
                .Bind(facilities => GetAccount(accountId, facilities, cancellationToken))
            );


        Result Validate()
        {
            var validator = new AccountRequestValidator(context);
            return validator.ValidateAdd(request).ToResult();
        }


        async Task<Result<Account>> AddAccount()
        {
            var now = DateTime.UtcNow;

            var newAccount = new Account
            {
                Address = request.Address,
                Created = now,
                Email = request.Email ?? context.Email ?? string.Empty,
                IsActive = true,
                Modified = now,
                Name = request.Name,
                Phone = request.Phone ?? string.Empty,
                OperatingName = request.OperatingName ?? request.Name
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync(cancellationToken);

            await _stripeAccountService.AddForAccount(newAccount, cancellationToken);

            return newAccount;
        }

        async Task<Result<(int, int)>> AddDefaultFacility(Account account)
        {
            var (_, isFailure, facilityId) = await _facilityService.AddDefault(account.Id, account.OperatingName, cancellationToken);

            return isFailure
                ? Result.Failure<(int, int)>("Default facility hadn't been created.")
                : (account.Id, facilityId);
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


        void ClearMemberContextCache()
            => _memberContextCacheService.Remove(context.IdentityHash);
    }


    public Task<Result<AccountResponse>> Get(MemberContext context, int accountId, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Map(() => _facilityService.Get(accountId, cancellationToken))
            .Bind(facilities => GetAccount(accountId, facilities, cancellationToken))
            .Check(response => response.IsActive ? Result.Success() : Result.Failure("The account is switched off."));

        Result Validate()
        {
            var validator = new AccountRequestValidator(context);
            return validator.ValidateGet(AccountRequest.CreateEmpty(accountId)).ToResult();
        }
    }


    public Task<Result<AccountResponse>> Update(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(UpdateAccount)
            .Map(() => _facilityService.Get(request.Id!.Value, cancellationToken))
            .Bind(facilities => GetAccount(request.Id!.Value, facilities, cancellationToken));


        Result Validate()
        {
            var validator = new AccountRequestValidator(context);
            return validator.ValidateUpdate(request).ToResult();
        }


        async Task<Result> UpdateAccount()
        {
            var existingAccount = await _context.Accounts
                .Where(a => a.Id == request.Id!.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (existingAccount is null)
                return Result.Failure("The account was not found.");

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


    private async Task<Result<AccountResponse>> GetAccount(int accountId, List<FacilityResponse> facilities, CancellationToken cancellationToken)
        => await _context.Accounts
            .Where(a => a.Id == accountId && a.IsActive)
            .Select(AccountProjection(facilities))
            .SingleOrDefaultAsync(cancellationToken);


    private static Expression<Func<Account, AccountResponse>> AccountProjection(List<FacilityResponse> facilities)
        => a => new AccountResponse(a.Id, a.Name, a.OperatingName, a.Address, a.AvatarUrl, a.Email, a.Phone, a.IsActive, facilities);


    private readonly AetherDbContext _context;
    private readonly IMemberContextCacheService _memberContextCacheService;
    private readonly IFacilityService _facilityService;
    private readonly IStripeAccountService _stripeAccountService;
}