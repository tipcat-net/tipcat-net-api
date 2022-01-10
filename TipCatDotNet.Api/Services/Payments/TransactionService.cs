using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Stripe;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Payments.Enums;
using System.Linq.Expressions;
using HappyTravel.Money.Models;
using HappyTravel.Money.Enums;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;

namespace TipCatDotNet.Api.Services.Payments;

public class TransactionService : ITransactionService
{
    public TransactionService(AetherDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger<TransactionService>();
    }


    public async Task<Result> Add(PaymentIntent paymentIntent, string? message, CancellationToken cancellationToken = default)
    {
        var memberId = int.Parse(paymentIntent.Metadata["MemberId"]);

        var facilityId = await _context.Members
            .Where(m => m.Id == memberId)
            .Select(m => m.FacilityId)
            .SingleOrDefaultAsync();

        if (facilityId == null)
        {
            var error = "Member who receive payment doesn't belong to any facility.";
            _logger.LogMemberBelongFacilityFailure(error);
            return Result.Failure(error);
        }

        var now = DateTime.UtcNow;

        var newTransaction = new Transaction
        {
            Amount = MoneyConverting.ToFractionalUnits(paymentIntent),
            Currency = paymentIntent.Currency,
            MemberId = memberId,
            FacilityId = facilityId.Value,
            Message = message ?? string.Empty,
            PaymentIntentId = paymentIntent.Id,
            State = paymentIntent.Status,
            Created = now,
            Modified = now,
        };

        _context.Transactions.Add(newTransaction);
        await _context.SaveChangesAsync(cancellationToken);

        // var (_, isFailure, error) = await SetPaymentTime(memberId);
        // if (isFailure)
        //     return Result.Failure(error);

        return Result.Success();


        async Task<Result> SetPaymentTime(int memberId)
        {
            var stripeAccount = await _context.StripeAccounts
                .SingleOrDefaultAsync(sa => sa.MemberId == memberId, cancellationToken);

            if (stripeAccount is null)
                return Result.Failure($"Member with ID - {memberId} has no any connected stripe accounts!");

            var rand = new Random();
            var now = DateTime.UtcNow;

            stripeAccount.LastReceived = now.AddMinutes(rand.Next(15, 30));

            _context.StripeAccounts.Update(stripeAccount);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }


    public async Task<Result<List<TransactionResponse>>> Get(MemberContext context, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Where(t => t.MemberId == context.Id)
            .Select(TransactionProjection())
            .ToListAsync(cancellationToken);


    public Task<Result<List<TransactionResponse>>> Get(MemberContext memberContext, int facilityId, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(GetTransactions);


        Result Validate()
        {
            var validator = new FacilityRequestValidator(memberContext, _context);
            var validationResult = validator.ValidateGetOrUpdate(new FacilityRequest(facilityId, memberContext.AccountId));
            return validationResult.ToResult();
        }


        async Task<Result<List<TransactionResponse>>> GetTransactions()
            => await _context.Transactions
                .Where(t => t.FacilityId == facilityId)
                .Select(TransactionProjection())
                .ToListAsync(cancellationToken);
    }


    public Task<Result<List<FacilityTransactionResponse>>> GetByAccount(MemberContext memberContext, int accountId,
        CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(GetTransactions);


        Result Validate()
        {
            var validator = new AccountRequestValidator(memberContext);
            var validationResult = validator.ValidateGet(AccountRequest.CreateEmpty(accountId));
            return validationResult.ToResult();
        }


        async Task<Result<List<FacilityTransactionResponse>>> GetTransactions()
        {
            var now = DateTime.UtcNow;

            var facilities = await _context.Facilities
                .Where(f => f.AccountId == accountId)
                .Select(FacilityProjection())
                .ToListAsync(cancellationToken);

            var transactions = await _context.Transactions
                .GroupBy(t => new { t.FacilityId, t.Currency })
                .Select(x => new
                {
                    Keys = x.Key,
                    Total = x.Sum(x => x.Amount)
                })
                .ToDictionaryAsync(x => x.Keys.FacilityId,
                    x => new MoneyAmount(x.Total, (Currencies)Enum.Parse(typeof(Currencies), x.Keys.Currency, true)));

            var result = new List<FacilityTransactionResponse>(facilities.Count);
            foreach (var facility in facilities)
            {
                transactions.TryGetValue(facility.Id, out var totalAmount);
                result.Add(new FacilityTransactionResponse(facility, totalAmount));
            }

            return result;
        }
    }


    public async Task<Result> Update(PaymentIntent paymentIntent, string? message, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var transaction = await _context.Transactions
            .Where(t => t.PaymentIntentId == paymentIntent.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (transaction is null)
            return Result.Failure("The transaction was not found.");

        transaction.Amount = MoneyConverting.ToFractionalUnits(paymentIntent);
        transaction.Currency = paymentIntent.Currency;
        transaction.Message = message ?? transaction.Message;
        transaction.State = paymentIntent.Status;
        transaction.Modified = now;

        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    private static Expression<Func<Transaction, TransactionResponse>> TransactionProjection()
        => transaction => new TransactionResponse(new MoneyAmount(transaction.Amount, MoneyConverting.ToCurrency(transaction.Currency)),
            transaction.MemberId, transaction.FacilityId, transaction.Message, transaction.State, transaction.Created);


    private static Expression<Func<Facility, FacilityResponse>> FacilityProjection()
        => facility => new FacilityResponse(facility.Id, facility.Name, facility.Address, facility.AccountId, facility.AvatarUrl, null);


    private readonly AetherDbContext _context;
    private readonly ILogger<TransactionService> _logger;
}