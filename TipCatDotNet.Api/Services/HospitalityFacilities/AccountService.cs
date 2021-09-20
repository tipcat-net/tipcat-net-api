using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Infrastructure.FunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class AccountService : IAccountService
    {
        public AccountService(AetherDbContext context, IMemberContextCacheService memberContextCacheService)
        {
            _context = context;
            _memberContextCacheService = memberContextCacheService;
        }


        public Task<Result<AccountResponse>> Add(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Ensure(() => context.AccountId is null, "This member already has an account.")
                .Bind(() => ValidateAccountParameters(request))
                .BindWithTransaction(_context, () => AddAccount()
                    .Bind(AttachToMember)
                    .Tap(ClearCache)
                    .Bind(accountId => GetAccount(accountId, cancellationToken)));


            async Task<Result<Account>> AddAccount()
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

                return newAccount;
            }


            async Task<Result<int>> AttachToMember(Account account)
            {
                _context.AccountMembers.Add(new AccountMember
                {
                    AccountId = account.Id,
                    MemberId = context.Id
                });

                await _context.SaveChangesAsync(cancellationToken);

                return account.Id;
            }


            void ClearCache() 
                => _memberContextCacheService.Remove(context.IdentityHash);
        }


        public Task<Result<AccountResponse>> Get(MemberContext context, int accountId, CancellationToken cancellationToken = default)
            => Result.Success()
                .Ensure(() => context.AccountId is not null, "The member has no accounts.")
                .Ensure(() => context.AccountId == accountId, "The member has no access to this account.")
                .Bind(() => GetAccount(accountId, cancellationToken))
                .Check(response => response.IsActive ? Result.Success() : Result.Failure("The account is switched off."));


        public Task<Result<AccountResponse>> Update(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Ensure(() => request.Id is not null, "Provided account details have no ID.")
                .Ensure(() => request.Id == context.AccountId, "An account doesn't belongs to the current member.")
                .Bind(() => ValidateAccountParameters(request))
                .Bind(UpdateAccount)
                .Bind(() => GetAccount((int)request.Id!, cancellationToken));


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
    }
}
