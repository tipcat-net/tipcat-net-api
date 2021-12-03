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
using HappyTravel.Money.Enums;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Money.Models;
using HappyTravel.Money.Extensions;

namespace TipCatDotNet.Api.Services.Payments
{
    public class TransactionService : ITransactionService
    {
        public TransactionService(AetherDbContext context)
        {
            _context = context;
        }


        public async Task<Result> Add(string? message, PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var newTransaction = new Transaction
            {
                Amount = ToFractionalUnits(paymentIntent),
                Currency = paymentIntent.Currency,
                MemberId = int.Parse(paymentIntent.Metadata["MemberId"]),
                Message = message ?? string.Empty,
                PaymentIntentId = paymentIntent.Id,
                State = paymentIntent.Status,
                Created = now,
                Modified = now,
            };

            _context.Transactions.Add(newTransaction);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        public Task<Result<List<TransactionResponse>>> Get(MemberContext context, int skip, int top, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Bind(GetSucceededTransactions);


            async Task<Result<List<TransactionResponse>>> GetSucceededTransactions()
                => await _context.Transactions
                    .Where(t => t.MemberId == context.Id && t.State == "succeeded")
                    .OrderByDescending(t => t.Created)
                    .Skip(skip)
                    .Take(top)
                    .Select(TransactionProjection())
                    .ToListAsync(cancellationToken);
        }


        public async Task<Result> Update(string? message, PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var transaction = await _context.Transactions
                   .Where(t => t.PaymentIntentId == paymentIntent.Id)
                   .SingleOrDefaultAsync(cancellationToken);

            if (transaction is null)
                return Result.Failure("The transaction was not found.");

            transaction.Amount = ToFractionalUnits(paymentIntent);
            transaction.Currency = paymentIntent.Currency;
            transaction.Message = message ?? transaction.Message;
            transaction.State = paymentIntent.Status;
            transaction.Modified = now;

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        private static decimal ToFractionalUnits(in PaymentIntent paymentIntent)
            => paymentIntent.Amount / (decimal)Math.Pow(10, ToCurrency(paymentIntent.Currency).GetDecimalDigitsCount());


        private static Currencies ToCurrency(string currency)
            => Enum.Parse<Currencies>(currency.ToUpper());


        private static Expression<Func<Transaction, TransactionResponse>> TransactionProjection()
            => transaction => new TransactionResponse(new MoneyAmount(transaction.Amount, ToCurrency(transaction.Currency)),
                transaction.MemberId, transaction.Message, transaction.State, transaction.Created);


        private readonly AetherDbContext _context;
    }
}