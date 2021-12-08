using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Stripe;

namespace TipCatDotNet.Api.Services.Payments;

public class PayoutService : IPayoutService
{
    public PayoutService(AetherDbContext context, Stripe.PayoutService payoutService, BalanceService balanceService, ILoggerFactory loggerFactory)
    {
        _context = context;
        _payoutService = payoutService;
        _balanceService = balanceService;
        _logger = loggerFactory.CreateLogger<PayoutService>();
    }


    public async Task<Result> PayOut(CancellationToken cancellationToken = default)
    {
        var allStripeAccounts = await _context.StripeAccounts
            .Where(sa => sa.LastReceived > sa.LastPaidOut)
            .Join(_context.Members, s => s.MemberId, m => m.Id, (s, m) => s)
            .ToListAsync(cancellationToken);

        allStripeAccounts.ForEach(async stripeAccount =>
        {
            await PayOutInternal(stripeAccount);
        });

        return Result.Success(); ;


        async Task<Result<Balance>> GetBalance(string stripeAccountId)
        {
            try
            {
                var requestOptions = new RequestOptions();
                requestOptions.StripeAccount = stripeAccountId;
                return await _balanceService.GetAsync(requestOptions, cancellationToken);
            }
            catch (StripeException ex)
            {
                return Result.Failure<Balance>(ex.Message);
            }
        }


        async Task<Result> SetPayOutTime(StripeAccount stripeAccount)
        {
            var now = DateTime.UtcNow;

            stripeAccount.LastPaidOut = now;

            _context.StripeAccounts.Update(stripeAccount);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        async Task<Result> PayOutInternal(StripeAccount stripeAccount)
        {
            Result result = Result.Success();
            var (_, isFailure, balance, error) = await GetBalance(stripeAccount.StripeId);

            if (isFailure)
                _logger.LogWarning(error);

            balance.Available.ForEach(async ba =>
            {
                try
                {
                    var createOptions = new PayoutCreateOptions()
                    {
                        Amount = ba.Amount,
                        Currency = ba.Currency
                    };

                    var requestOptions = new RequestOptions() { StripeAccount = stripeAccount.StripeId };
                    var payOut = await _payoutService.CreateAsync(createOptions, requestOptions, cancellationToken);

                    var (_, isFailure, error) = await SetPayOutTime(stripeAccount);
                    if (isFailure)
                        _logger.LogWarning(error);
                }
                catch (StripeException ex)
                {
                    _logger.LogWarning(ex.Message);
                }
            });
            return result;
        }
    }


    private readonly AetherDbContext _context;
    private readonly Stripe.PayoutService _payoutService;
    private readonly BalanceService _balanceService;
    private readonly ILogger<PayoutService> _logger;
}
