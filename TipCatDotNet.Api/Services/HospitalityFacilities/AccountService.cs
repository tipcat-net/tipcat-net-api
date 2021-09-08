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
        public AccountService(AetherDbContext context)
        {
            _context = context;
        }


        public async Task<Result<AccountResponse>> Add(MemberContext context, AccountRequest request, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => context.AccountId is null, "This member already has an account.")
                .BindWithTransaction(_context, async () => await AddAccount()
                    .Bind(async (account) => await AttachMemberToAccount(account))
                    .Tap(ClearCache)
                    .Bind(GetResponse));


            async Task<Result<Account>> AddAccount()
            {
                var now = DateTime.UtcNow;

                var newAccount = new Account
                {
                    Address = request.Address,
                    CommercialName = request.CommercialName ?? request.Name,
                    Created = now,
                    Email = request.Email,
                    Modified = now,
                    Name = request.Name,
                    Phone = request.Phone,
                    State = ModelStates.Active
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync(cancellationToken);

                return newAccount;
            }


            async Task<Result<Account>> AttachMemberToAccount(Account account)
            {
                _context.AccountMembers.Add(new AccountMember
                {
                    AccountId = account.Id,
                    MemberId = context.Id
                });

                await _context.SaveChangesAsync(cancellationToken);

                return account;
            }


            Task ClearCache()
            {
                return Task.CompletedTask;
            }


            Result<AccountResponse> GetResponse(Account account)
            {
                return new AccountResponse(account.Id, true);
            }
        }


        public async Task<Result<AccountResponse>> Get(MemberContext context, int accountId, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Ensure(() => context.AccountId is not null, "The member has no accounts.")
                .Ensure(() => context.AccountId == accountId, "The member has no access to this account.")
                .Bind(GetAccount)
                .Check(response => response.IsActive ? Result.Success() : Result.Failure("The account is switched off."));


            async Task<Result<AccountResponse>> GetAccount()
                => await _context.Accounts
                    .Where(a => a.Id == accountId && a.State == ModelStates.Active)
                    .Select(a => new AccountResponse(a.Id, a.State == ModelStates.Active))
                    .SingleOrDefaultAsync(cancellationToken);
        }


        private readonly AetherDbContext _context;
    }
}
