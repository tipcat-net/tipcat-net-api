using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.Api.Services.Payments;

public class PayoutService : IPayoutService
{
    public PayoutService(AetherDbContext context, Stripe.PayoutService payoutService)
    {
        _context = context;
        _payoutService = payoutService;
    }


    public Task<Result> PayoutAll(CancellationToken cancellationToken = default)
    {
        return Result.Success()
            .Bind(PayoutByUsers);


        async Task<Result> PayoutByUsers()
        {
            Result result = Result.Success();
            var allStripeAccounts = await _context.StripeAccounts
                .Join(_context.Members.Where(m => m.IsActive), s => s.MemberId, m => m.Id, (s, m) => s.StripeId)
                .ToListAsync(cancellationToken);

            allStripeAccounts.ForEach(async stripeAccointId =>
            {
                var (_, IsFailure, error) = await Payout(stripeAccointId);
                if (IsFailure)
                    result = Result.Failure(error);
            });

            return result;
        }


        async Task<Result<Balance>> GetBalance(string stripeAccointId)
        {
            try
            {
                var requestOptions = new RequestOptions();
                requestOptions.StripeAccount = stripeAccointId;
                var service = new BalanceService();
                Balance balance = await service.GetAsync(requestOptions, cancellationToken);
                return balance;
            }
            catch (StripeException ex)
            {
                return Result.Failure<Balance>(ex.Message);
            }
        }


        async Task<Result> Payout(string stripeAccointId)
        {
            Result result = Result.Success();
            var (_, IsFailure, balance, error) = await GetBalance(stripeAccointId);

            if (IsFailure)
                return Result.Failure<Balance>(error);

            balance.Available.ForEach(async ba =>
            {
                try
                {
                    var createOptions = new PayoutCreateOptions()
                    {
                        Amount = ba.Amount,
                        Currency = ba.Currency
                    };

                    var requestOptions = new RequestOptions() { StripeAccount = stripeAccointId };
                    var payout = await _payoutService.CreateAsync(createOptions, requestOptions, cancellationToken);
                }
                catch (StripeException ex)
                {
                    result = Result.Failure<Balance>(ex.Message);
                }
            });
            return result;
        }
    }


    private readonly AetherDbContext _context;
    private readonly Stripe.PayoutService _payoutService;
}
