using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Analitics;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.Analitics;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;

namespace TipCatDotNet.Api.Services.Analitics;

public class AccountStatsService : IAccountStatsService
{
    public AccountStatsService(AetherDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger<AccountStatsService>();
    }


    public async Task AddOrUpdate(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var accountId = await _context.Facilities
            .Where(m => m.Id == transaction.FacilityId)
            .Select(m => m.AccountId)
            .SingleAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var accountStats = await _context.AccountsStats
            .Where(a => a.AccountId == accountId)
            .SingleOrDefaultAsync(cancellationToken);

        if (accountStats == null)
        {
            var message = "There is no any AccountStats related with target accountId. So it will be created.";
            _logger.LogAccountStatsDoesntExist(message);

            accountStats = AccountStats.Empty(accountId, now);
        }
        else if (accountStats.CurrentDate != now)
        {
            accountStats = AccountStats.Reset(accountStats, now);
        }

        accountStats.TransactionsCount += 1;
        accountStats.AmountPerDay += transaction.Amount;
        accountStats.TotalAmount += transaction.Amount;
        accountStats.Modified = now;

        _context.AccountsStats.Update(accountStats);
        await _context.SaveChangesAsync(cancellationToken);
    }


    public async Task<Result<AccountStatsResponse>> Get(int accountId, CancellationToken cancellationToken = default)
    {
        var accountStatsResponse = await _context.AccountsStats
            .Where(a => a.AccountId == accountId)
            .Select(AccountStatsResponseProjection())
            .SingleOrDefaultAsync(cancellationToken);

        if (accountStatsResponse.Equals(default))
        {
            var message = "There is no any AccountStats related with target accountId.";
            _logger.LogAccountStatsDoesntExist(message);

            return Result.Failure<AccountStatsResponse>(message);
        }

        return accountStatsResponse;


        Expression<Func<AccountStats, AccountStatsResponse>> AccountStatsResponseProjection()
            => accountStats => new AccountStatsResponse(accountStats.Id, accountStats.TransactionsCount,
                accountStats.AmountPerDay, accountStats.TotalAmount, accountStats.CurrentDate);
    }


    public Task<Result<List<FacilityStatsResponse>>> GetFacilities(MemberContext memberContext, int accountId,
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


        async Task<Result<List<FacilityStatsResponse>>> GetTransactions()
            => _context.Transactions
                .Join(_context.Facilities.Where(f => f.AccountId == accountId),
                    t => t.FacilityId, f => f.Id, (t, f) => t)
                .GroupBy(GroupingProjection(), new FacilityComparer())
                .Select(FacilityAmountProjection())
                .GroupBy(f => f.FacilityId)
                .Select(FacilityStatsProjection())
                .ToList();


        Func<IGrouping<int, FacilityAmount>, FacilityStatsResponse> FacilityStatsProjection()
            => grouping => new FacilityStatsResponse(
                grouping.Key,
                grouping.Select(fa => fa.Amount).ToList()
            );


        Func<IGrouping<GroupByFacility, Transaction>, FacilityAmount> FacilityAmountProjection()
            => groupingFacility => new FacilityAmount(
                groupingFacility.Key.FacilityId,
                new MoneyAmount(groupingFacility.Sum(item => item.Amount), MoneyConverting.ToCurrency(groupingFacility.Key.Currency))
            );


        Func<Transaction, GroupByFacility> GroupingProjection()
            => t => new GroupByFacility(t.FacilityId, t.Currency);
    }


    private readonly AetherDbContext _context;
    private readonly ILogger<AccountStatsService> _logger;

    private class GroupByFacility
    {
        public GroupByFacility(int facilityId, string currency)
        {
            FacilityId = facilityId;
            Currency = currency;
        }


        public int FacilityId { get; set; }
        public string Currency { get; set; }
    }

    private class FacilityAmount
    {
        public FacilityAmount(int facilityId, MoneyAmount amount)
        {
            FacilityId = facilityId;
            Amount = amount;
        }


        public int FacilityId { get; set; }
        public MoneyAmount Amount { get; set; }
    }

    private class FacilityComparer : IEqualityComparer<GroupByFacility>
    {
        public bool Equals(GroupByFacility x, GroupByFacility y)
            => x.FacilityId == y.FacilityId &&
            x.Currency.Equals(y.Currency, StringComparison.OrdinalIgnoreCase);


        public int GetHashCode(GroupByFacility x)
            => x.FacilityId.GetHashCode();
    }
}