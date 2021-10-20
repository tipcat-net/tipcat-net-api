using System;
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
                if (validationResult.IsValid)
                    return Result.Success();

                return validationResult.ToFailureResult();
            }


            async Task<Result<int>> AddAccount()
            {
                var now = DateTime.UtcNow;

                var newAccount = new Account
                {
                    Address = request.Address,
                    CommercialName = request.CommercialName ?? request.Name,
                    Created = now,
                    Email = request.Email ?? context.Email ?? string.Empty,
                    Modified = now,
                    Name = request.Name,
                    Phone = request.Phone,
                    State = ModelStates.Active
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync(cancellationToken);

                return newAccount.Id;
            }

            async Task<Result<(int, int)>> AddDefaultFacility(int accountId)
            {
                var (_, isFailure, facilityId) = await _facilityService.AddDefault(accountId);

                if (isFailure)
                {
                    return Result.Failure<(int, int)>("Default facility hadn't been created.");
                }

                return (accountId, facilityId);
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
                var validationResult = validator.ValidateGet(new AccountRequest(accountId));
                if (validationResult.IsValid)
                    return Result.Success();

                return validationResult.ToFailureResult();
            }
        }


        public Task<Result<AccountResponse>> Update(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
        {
            return Validate()
                .Bind(UpdateAccount)
                .Bind(() => GetAccount((int)request.Id!, cancellationToken));


            Result Validate()
            {
                var validator = new AccountRequestValidator(context);
                var validationResult = validator.ValidateUpdate(request);
                if (validationResult.IsValid)
                    return Result.Success();

                return validationResult.ToFailureResult();
            }


            async Task<Result> UpdateAccount()
            {
                var existingAccount = await _context.Accounts
                    .Where(a => a.Id == (int)request.Id!)
                    .SingleOrDefaultAsync(cancellationToken);

                existingAccount.Address = request.Address;
                existingAccount.CommercialName = request.CommercialName ?? string.Empty;
                existingAccount.Email = request.Email ?? string.Empty;
                existingAccount.Modified = DateTime.UtcNow;
                existingAccount.Name = request.Name;
                existingAccount.Phone = request.Phone;

                _context.Accounts.Update(existingAccount);

                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        private async Task<Result<AccountResponse>> GetAccount(int accountId, CancellationToken cancellationToken)
            => await _context.Accounts
                .Where(a => a.Id == accountId && a.State == ModelStates.Active)
                .Select(a => new AccountResponse(a.Id, a.Name, a.CommercialName, a.Address, a.Email, a.Phone, a.State == ModelStates.Active))
                .SingleOrDefaultAsync(cancellationToken);


        private static Result ValidateAccountParameters(AccountRequest request)
            => Result.Success()
                .Ensure(() => !string.IsNullOrWhiteSpace(request.Name), "An account name should be specified.")
                .Ensure(() => !string.IsNullOrWhiteSpace(request.Address), "An account address should be specified.")
                .Ensure(() => !string.IsNullOrWhiteSpace(request.Phone), "A contact phone number should be specified.");


        private readonly AetherDbContext _context;
        private readonly IMemberContextCacheService _memberContextCacheService;
        private readonly IFacilityService _facilityService;
    }
}
