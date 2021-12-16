using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Common.Enums;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Payments;
using TipCatDotNet.Api.Models.Payments.Enums;

namespace TipCatDotNet.Api.Filters.Payment;

public class TransactionSorting : ITransactionSorting
{
    public TransactionSorting(AetherDbContext context)
    {
        _context = context;
    }


    public async Task<Result<List<TransactionResponse>>> ByCreatedASC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken)
        => await _context.Transactions
            // .Where(t => t.MemberId == context.Id && t.State == "succeeded")
            .Where(t => t.MemberId == memberId)
            .OrderBy(t => t.Created)
            .Skip(skip)
            .Take(top)
            .Select(TransactionProjection())
            .ToListAsync(cancellationToken);


    public async Task<Result<List<TransactionResponse>>> ByCreatedDESC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken)
        => await _context.Transactions
            // .Where(t => t.MemberId == context.Id && t.State == "succeeded")
            .Where(t => t.MemberId == memberId)
            .OrderByDescending(t => t.Created)
            .Skip(skip)
            .Take(top)
            .Select(TransactionProjection())
            .ToListAsync(cancellationToken);


    public async Task<Result<List<TransactionResponse>>> ByAmountASC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken)
        => await _context.Transactions
            // .Where(t => t.MemberId == context.Id && t.State == "succeeded")
            .Where(t => t.MemberId == memberId)
            .OrderBy(t => t.Amount)
            .Skip(skip)
            .Take(top)
            .Select(TransactionProjection())
            .ToListAsync(cancellationToken);


    public async Task<Result<List<TransactionResponse>>> ByAmountDESC(int memberId, int skip, int top,
        TransactionFilterProperty property, SortVariant variant, CancellationToken cancellationToken)
        => await _context.Transactions
            // .Where(t => t.MemberId == context.Id && t.State == "succeeded")
            .Where(t => t.MemberId == memberId)
            .OrderByDescending(t => t.Amount)
            .Skip(skip)
            .Take(top)
            .Select(TransactionProjection())
            .ToListAsync(cancellationToken);


    private static Expression<Func<Transaction, TransactionResponse>> TransactionProjection()
        => transaction => new TransactionResponse(new MoneyAmount(transaction.Amount, MoneyConverting.ToCurrency(transaction.Currency)),
            transaction.MemberId, transaction.Message, transaction.State, transaction.Created);


    private readonly AetherDbContext _context;
}
