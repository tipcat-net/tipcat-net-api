using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using TipCatDotNet.Api.Data;
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


        public async Task<Result<string>> Add(MemberRequest request, CancellationToken cancellationToken)
        {
            var options = new AccountCreateOptions
            {
                Country = "AE",
                Type = "standart", // TODO: leave it for now
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


        public async Task<Result<StripeAccountResponse>> Retrieve(MemberRequest request, CancellationToken cancellationToken)
        {
            var member = await _context.Members
                .Where(m => m.Id == request.Id)
                .SingleOrDefaultAsync(cancellationToken);

            try
            {
                var account = await _accountService.GetAsync(member!.StripeAccountId, cancellationToken: cancellationToken);
                if (account.Individual.Metadata["MemberId"] == member.Id.ToString())
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
            var member = await _context.Members
                .Where(m => m.Id == request.Id)
                .SingleOrDefaultAsync(cancellationToken);

            var (_, isFailure, isMatch, error) = await AreAccountsMatch(member!.Id, member!.StripeAccountId, cancellationToken);

            if (isFailure)
                return Result.Failure(error);

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
                var updatedAccount = await _accountService.UpdateAsync(member!.StripeAccountId, updateOptions, cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (StripeException ex)
            {
                return Result.Failure<PaymentIntent>(ex.Message);
            }
        }


        public async Task<Result> AttachDefaultExternal(PayoutMethodRequest request, CancellationToken cancellationToken)
        {
            var member = await _context.Members
                .Where(m => m.Id == request.MemberId)
                .SingleOrDefaultAsync(cancellationToken);

            var (_, isFailure, isMatch, error) = await AreAccountsMatch(member!.Id, member!.StripeAccountId, cancellationToken);

            if (isFailure)
                return Result.Failure(error);

            var attachOptions = new AccountUpdateOptions
            {
                ExternalAccount = request.Token
            };
            try
            {
                var updatedAccount = await _accountService.UpdateAsync(member!.StripeAccountId, attachOptions, cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (StripeException ex)
            {
                return Result.Failure<PaymentIntent>(ex.Message);
            }
        }


        /// <summary>
        /// Remove stripe account.
        /// Accounts created using test-mode keys can be deleted at any time.
        /// Custom or Express accounts created using live-mode keys can only be deleted once all balances are zero.
        /// </summary>
        public async Task<Result> Remove(int memberId, CancellationToken cancellationToken)
        {
            var member = await _context.Members
                .Where(m => m.Id == memberId)
                .SingleOrDefaultAsync(cancellationToken);

            var (_, isFailure, isMatch, error) = await AreAccountsMatch(member!.Id, member!.StripeAccountId, cancellationToken);

            if (isFailure)
                return Result.Failure(error);

            try
            {
                var updatedAccount = await _accountService.DeleteAsync(member!.StripeAccountId, cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (StripeException ex)
            {
                return Result.Failure<PaymentIntent>(ex.Message);
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