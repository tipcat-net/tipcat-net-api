using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Stripe;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public class StripeAccountService : IStripeAccountService
    {
        public StripeAccountService(AetherDbContext context, Stripe.AccountService accountService, IOptions<StripeOptions> stripeOptions)
        {
            _context = context;
            _accountService = accountService;
            _stripeOptions = stripeOptions;
        }


        public Task<Result> Add(MemberRequest request, CancellationToken cancellationToken)
        {
            return Result.Success()
                .Bind(CreateStripeAccount)
                .Bind(CreateRelatedAccount);


            async Task<Result<string>> CreateStripeAccount()
            {
                var options = new AccountCreateOptions
                {
                    Country = "AE",
                    Type = "custom", // TODO: leave it for now
                    Individual = new AccountIndividualOptions
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        Metadata = new Dictionary<string, string>()
                    {
                        { "MemberId", request.Id!.ToString() ?? string.Empty },
                    },
                    },
                    Capabilities = new AccountCapabilitiesOptions
                    {
                        CardPayments = new AccountCapabilitiesCardPaymentsOptions
                        {
                            Requested = true,
                        },
                        Transfers = new AccountCapabilitiesTransfersOptions
                        {
                            Requested = true,
                        },
                        // TODO: Figure out which cababilities (Payment_methods) account requested
                    }
                };
                try
                {
                    var account = await _accountService.CreateAsync(options, cancellationToken: cancellationToken);
                    return account.Id;
                }
                catch (StripeException ex)
                {
                    return Result.Failure<string>(ex.Message);
                }
            }


            async Task<Result> CreateRelatedAccount(string accountId)
            {
                var now = DateTime.UtcNow;

                var newRelatedAccount = new StripeAccount
                {
                    StripeId = accountId,
                    MemberId = (int)request.Id!
                };

                _context.StripeAccounts.Add(newRelatedAccount);
                await _context.SaveChangesAsync(cancellationToken);
                _context.DetachEntities();

                return Result.Success();
            }
        }


        public async Task<Result<StripeAccountResponse>> Retrieve(MemberRequest request, CancellationToken cancellationToken)
        {
            var stripeAccount = await _context.StripeAccounts
                .SingleOrDefaultAsync(s => s.MemberId == request.Id, cancellationToken);

            if (stripeAccount == null)
                return Result.Failure<StripeAccountResponse>("This is not presented member's account!");

            try
            {
                var account = await _accountService.GetAsync(stripeAccount.StripeId, cancellationToken: cancellationToken);
                if (account.Individual.Metadata["MemberId"] == request.Id.ToString())
                    return Result.Failure<StripeAccountResponse>("This is not presented member's account!");

                return new StripeAccountResponse(account.Id);
            }
            catch (StripeException ex)
            {
                return Result.Failure<StripeAccountResponse>(ex.Message);
            }
        }


        public async Task<Result> Update(MemberRequest request, CancellationToken cancellationToken)
        {
            var stripeAccount = await _context.StripeAccounts
                .SingleOrDefaultAsync(s => s.MemberId == request.Id, cancellationToken);

            if (stripeAccount == null)
                return Result.Failure("This is not presented member's account!");

            // var (_, isFailure, isMatch, error) = await AreAccountsMatch(stripeAccount.MemberId, stripeAccount.StripeId, cancellationToken);

            // if (isFailure)
            //     return Result.Failure(error);

            var updateOptions = new AccountUpdateOptions
            {
                Individual = new AccountIndividualOptions
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email
                }
            };
            try
            {
                var updatedAccount = await _accountService.UpdateAsync(stripeAccount.StripeId, updateOptions, cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (StripeException ex)
            {
                return Result.Failure(ex.Message);
            }
        }


        public async Task<Result> AttachDefaultExternal(PayoutMethodRequest request, CancellationToken cancellationToken)
        {
            var stripeAccount = await _context.StripeAccounts
                .SingleOrDefaultAsync(s => s.MemberId == request.MemberId, cancellationToken);

            if (stripeAccount == null)
                return Result.Failure("This is not presented member's account!");

            var (_, isFailure, isMatch, error) = await AreAccountsMatch(stripeAccount.MemberId, stripeAccount.StripeId, cancellationToken);

            if (isFailure)
                return Result.Failure(error);

            var attachOptions = new AccountUpdateOptions
            {
                ExternalAccount = request.Token
            };
            try
            {
                var updatedAccount = await _accountService.UpdateAsync(stripeAccount.StripeId, attachOptions, cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (StripeException ex)
            {
                return Result.Failure(ex.Message);
            }
        }


        /// <summary>
        /// Remove stripe account.
        /// Accounts created using test-mode keys can be deleted at any time.
        /// Custom or Express accounts created using live-mode keys can only be deleted once all balances are zero.
        /// </summary>
        public Task<Result> Remove(int memberId, CancellationToken cancellationToken)
        {
            return Result.Success()
                .Bind(RemoveStripeAccount)
                .Bind(RemoveRelatedAccount);


            async Task<Result<StripeAccount>> RemoveStripeAccount()
            {
                var stripeAccount = await _context.StripeAccounts
                .SingleOrDefaultAsync(s => s.MemberId == memberId, cancellationToken);

                if (stripeAccount == null)
                    return Result.Failure<StripeAccount>("This is not presented member's account!");

                // var (_, isFailure, isMatch, error) = await AreAccountsMatch(stripeAccount.MemberId, stripeAccount.StripeId, cancellationToken);

                // if (isFailure)
                //     return Result.Failure<StripeAccount>(error);

                try
                {
                    var deletedAccount = await _accountService.DeleteAsync(stripeAccount.StripeId, cancellationToken: cancellationToken);
                    return stripeAccount;
                }
                catch (StripeException ex)
                {
                    return Result.Failure<StripeAccount>(ex.Message);
                }
            }


            async Task<Result> RemoveRelatedAccount(StripeAccount account)
            {
                _context.StripeAccounts.Remove(account);
                await _context.SaveChangesAsync(cancellationToken);
                _context.DetachEntities();

                return Result.Success();
            }
        }


        private async Task<Result<bool>> AreAccountsMatch(int memberId, string accountId, CancellationToken cancellationToken)
        {
            try
            {
                var account = await _accountService.GetAsync(accountId, cancellationToken: cancellationToken);
                if (account.Individual.Metadata["MemberId"] == memberId.ToString())
                    return Result.Failure<bool>("This is not presented member's account!");

                return true;
            }
            catch (StripeException ex)
            {
                return Result.Failure<bool>(ex.Message);
            }
        }


        private readonly AetherDbContext _context;
        private readonly Stripe.AccountService _accountService;
        private readonly IOptions<StripeOptions> _stripeOptions;
    }
}