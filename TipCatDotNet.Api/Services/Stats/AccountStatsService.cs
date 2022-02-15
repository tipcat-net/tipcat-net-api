using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;
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

namespace TipCatDotNet.Api.Services.Stats;

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


    public Task<Result<AccountStatsResponse>> Get(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(GetAccountAnalitics);


        Result Validate()
        {
            var validator = new AccountRequestValidator(memberContext);
            var validationResult = validator.ValidateGet(AccountRequest.CreateEmpty(accountId));
            return validationResult.ToResult();
        }


        async Task<Result<AccountStatsResponse>> GetAccountAnalitics()
        {
            var facilities = await GetFacilities(accountId, cancellationToken);

            var accountStatsResponse = await _context.AccountsStats
                .Where(a => a.AccountId == accountId)
                .Select(AccountStatsResponseProjection(facilities))
                .SingleOrDefaultAsync(cancellationToken);

            if (accountStatsResponse.Equals(default))
            {
                var message = "There is no any AccountStats related with target accountId.";
                _logger.LogAccountStatsDoesntExist(message);

                return Result.Failure<AccountStatsResponse>(message);
            }

            return accountStatsResponse;
        }


        Expression<Func<AccountStats, AccountStatsResponse>> AccountStatsResponseProjection(List<FacilityStatsResponse>? facilities)
            => accountStats => new AccountStatsResponse(accountStats.Id, accountStats.TransactionsCount,
                accountStats.AmountPerDay, accountStats.TotalAmount, accountStats.CurrentDate, facilities);
    }


    public async Task<List<FacilityStatsResponse>> GetFacilities(int accountId, CancellationToken cancellationToken = default)
    {
        var resultList = _context.Transactions
                .Join(_context.Facilities.Where(f => f.AccountId == accountId),
                    t => t.FacilityId, f => f.Id, (t, f) => t)
                .GroupBy(GroupingProjection(), new FacilityComparer())
                .Select(MemberAmountProjection())
                .GroupBy(f => f.MemberId)
                .Select(GroupedMemberProjection())
                .GroupBy(m => m.FacilityId)
                .Select(FacilityStatsResponse())
                .ToList();

        return resultList;


        Func<IGrouping<int, GroupedMember>, FacilityStatsResponse> FacilityStatsResponse()
            => groupingMembers => new FacilityStatsResponse(
                groupingMembers.Key,
                groupingMembers.Select(MemberStatsProjection()).ToList(),
                groupingMembers
                    .SelectMany(g => g.Amounts)
                    .GroupBy(g => g.Currency, new CurrencyComparer())
                    .Select(g => new MoneyAmount(g.Sum(m => m.Amount), g.Key))
                    .ToList()
            );


        Func<GroupedMember, MemberStatsResponse> MemberStatsProjection()
            => groupedMember => new MemberStatsResponse(
                groupedMember.MemberId,
                groupedMember.Amounts
            );


        Func<IGrouping<int, MemberAmount>, GroupedMember> GroupedMemberProjection()
            => groupingAmounts => new GroupedMember(
                groupingAmounts.Key,
                groupingAmounts.First().FacilityId,
                groupingAmounts.Select(fa => fa.Amount).ToList()
            );


        Func<IGrouping<GroupByFacility, Transaction>, MemberAmount> MemberAmountProjection()
            => groupingFacilities => new MemberAmount(
                groupingFacilities.Key.MemberId,
                groupingFacilities.Key.FacilityId,
                new MoneyAmount(groupingFacilities.Sum(item => item.Amount), MoneyConverting.ToCurrency(groupingFacilities.Key.Currency))
            );


        Func<Transaction, GroupByFacility> GroupingProjection()
            => t => new GroupByFacility(t.FacilityId, t.MemberId, t.Currency);
    }


    private readonly AetherDbContext _context;
    private readonly ILogger<AccountStatsService> _logger;

    private class GroupByFacility
    {
        public GroupByFacility(int facilityId, int memberId, string currency)
        {
            FacilityId = facilityId;
            MemberId = memberId;
            Currency = currency;
        }


        public int FacilityId { get; set; }
        public int MemberId { get; set; }
        public string Currency { get; set; }
    }

    private class MemberAmount
    {
        public MemberAmount(int memberId, int facilityId, MoneyAmount amount)
        {
            MemberId = memberId;
            FacilityId = facilityId;
            Amount = amount;
        }


        public int MemberId { get; set; }
        public int FacilityId { get; set; }
        public MoneyAmount Amount { get; set; }
    }

    private class GroupedMember
    {
        public GroupedMember(int memberId, int facilityId, List<MoneyAmount> amounts)
        {
            MemberId = memberId;
            FacilityId = facilityId;
            Amounts = amounts;
        }


        public int MemberId { get; set; }
        public int FacilityId { get; set; }
        public List<MoneyAmount> Amounts { get; set; }
    }

    private class FacilityComparer : IEqualityComparer<GroupByFacility>
    {
        public bool Equals(GroupByFacility x, GroupByFacility y)
            => x.FacilityId == y.FacilityId && x.MemberId == y.MemberId &&
            x.Currency.Equals(y.Currency, StringComparison.OrdinalIgnoreCase);


        public int GetHashCode(GroupByFacility x)
            => x.FacilityId.GetHashCode();
    }

    private class CurrencyComparer : IEqualityComparer<Currencies>
    {
        public bool Equals(Currencies x, Currencies y)
            => x.Equals(y);


        public int GetHashCode(Currencies x)
            => x.GetHashCode();
    }
}