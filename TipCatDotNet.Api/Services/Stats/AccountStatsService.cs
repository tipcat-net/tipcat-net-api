using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public AccountStatsService(IConfiguration configuration, AetherDbContext context, ILoggerFactory loggerFactory, IExchangeRateService exchangeRateService)
    {
        _configuration = configuration;
        _context = context;
        _exchangeRateService = exchangeRateService;
        _logger = loggerFactory.CreateLogger<AccountStatsService>();
    }


    public async Task AddOrUpdate(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var targetCurrency = _configuration["Stripe:TargetCurrency"];

        var (accountId, sessionEndTime) = await _context.Facilities
            .Where(m => m.Id == transaction.FacilityId)
            .Select(m => new Tuple<int, TimeOnly>(m.AccountId, m.SessionEndTime))
            .SingleAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var accountStats = await _context.AccountsStats
            .Where(a => a.AccountId == accountId)
            .SingleOrDefaultAsync(cancellationToken);

        if (accountStats == null)
        {
            var message = "There is no any AccountStats related with target accountId. So it will be created.";
            _logger.LogAccountStatsDoesntExist(message);

            accountStats = AccountStats.Empty(accountId, targetCurrency, now);
        }
        else if (accountStats.CurrentDate != now || sessionEndTime < TimeOnly.FromDateTime(now))
        {
            accountStats = AccountStats.Reset(accountStats, now);
        }

        var (_, isFailure, rates, error) = await _exchangeRateService.GetExchangeRates(targetCurrency, cancellationToken);

        try
        {
            decimal rate = 1m;

            if (!transaction.Currency.Equals(targetCurrency))
                rate = rates.Rates[transaction.Currency];

            var amount = transaction.Amount * rate;

            accountStats.TransactionsCount += 1;
            accountStats.AmountPerDay += amount;
            accountStats.TotalAmount += amount;
            accountStats.Modified = now;

            _context.AccountsStats.Update(accountStats);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (RuntimeBinderException)
        {
            var message = $"Exchanging rate from {transaction.Currency} to {targetCurrency} doesn't exist!";
            _logger.LogExchangeRateException(message);
        }
    }


    public Task<Result<AccountStatsResponse>> Get(MemberContext memberContext, int accountId, CancellationToken cancellationToken = default)
    {
        var targetCurrency = _configuration["Stripe:TargetCurrency"];

        return Validate()
            .Bind(() => _exchangeRateService.GetExchangeRates(targetCurrency, cancellationToken))
            .Bind(rates => GetAccountAnalitics(rates));


        Result Validate()
        {
            var validator = new AccountRequestValidator(memberContext);
            var validationResult = validator.ValidateGet(AccountRequest.CreateEmpty(accountId));
            return validationResult.ToResult();
        }


        async Task<Result<AccountStatsResponse>> GetAccountAnalitics(DataRates rates)
        {
            var facilities = await GetFacilities(accountId, rates, targetCurrency, cancellationToken);

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
                accountStats.AmountPerDay, accountStats.TotalAmount, MoneyConverter.ToCurrency(accountStats.Currency), accountStats.CurrentDate, facilities);
    }


    private async Task<List<FacilityStatsResponse>> GetFacilities(int accountId, DataRates rates, string targetCurrency, CancellationToken cancellationToken = default)
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
                new MoneyAmount(
                    groupingMembers
                        .SelectMany(g => g.Amounts)
                        .Sum(a => a.Amount * CalculteExchangeRate(a.Currency, targetCurrency, rates)),
                    MoneyConverter.ToCurrency(targetCurrency)
                )
            );


        Func<GroupedMember, MemberStatsResponse> MemberStatsProjection()
            => groupedMember => new MemberStatsResponse(
                groupedMember.MemberId,
                new MoneyAmount(
                    groupedMember.Amounts.Sum(a => a.Amount * CalculteExchangeRate(a.Currency, targetCurrency, rates)),
                    MoneyConverter.ToCurrency(targetCurrency)
                )
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
                new MoneyAmount(groupingFacilities.Sum(item => item.Amount), MoneyConverter.ToCurrency(groupingFacilities.Key.Currency))
            );


        Func<Transaction, GroupByFacility> GroupingProjection()
            => t => new GroupByFacility(t.FacilityId, t.MemberId, t.Currency);
    }


    private decimal CalculteExchangeRate(Currencies currentCurrency, string targetCurrency, DataRates rates)
        => (MoneyConverter.ToStringCurrency(currentCurrency) != targetCurrency) ? rates.Rates.GetProperty(MoneyConverter.ToStringCurrency(currentCurrency)).GetDecimal() : 1m;


    private readonly AetherDbContext _context;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly IConfiguration _configuration;
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