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
using TipCatDotNet.Api.Filters.Pagination;

namespace TipCatDotNet.Api.Services.Payments
{
    public class TransactionService : ITransactionService
    {
        public TransactionService(AetherDbContext context)
        {
            _context = context;
        }


        public async Task<Result> Add(PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var newTransaction = new Transaction
            {
                Amount = paymentIntent.Amount,
                Currency = Enum.Parse<Currencies>(paymentIntent.Currency),
                MemberId = int.Parse(paymentIntent.Metadata["MemberId"]),
                PaymentIntentId = paymentIntent.Id,
                State = paymentIntent.Status,
                Created = paymentIntent.Created,
                Modified = now,
            };

            _context.Transactions.Add(newTransaction);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        public async Task<Result<List<TransactionResponse>>> Get(MemberContext context, PaginationFilter filter, CancellationToken cancellationToken = default)
            => await _context.Transactions
                .Where(t => t.MemberId == context.Id && t.State == "succeeded")
                .OrderByDescending(t => t.Created)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(TransactionProjection())
                .ToListAsync();


        public async Task<Result> Update(PaymentIntent paymentIntent, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var transaction = await _context.Transactions
                   .Where(t => t.PaymentIntentId == paymentIntent.Id)
                   .SingleOrDefaultAsync(cancellationToken);

            if (transaction is null)
                return Result.Failure("The transaction was not found.");

            transaction.Amount = paymentIntent.Amount;
            transaction.Currency = Enum.Parse<Currencies>(paymentIntent.Currency);
            transaction.State = paymentIntent.Status;
            transaction.Modified = now;

            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        private static Expression<Func<Transaction, TransactionResponse>> TransactionProjection()
            => transaction => new TransactionResponse(transaction.Amount, transaction.Currency, transaction.MemberId,
                transaction.State, transaction.Created);


        private readonly AetherDbContext _context;
    }
}