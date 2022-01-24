using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Analitics;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.Analitics;

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


    private readonly AetherDbContext _context;
    private readonly ILogger<AccountStatsService> _logger;
}