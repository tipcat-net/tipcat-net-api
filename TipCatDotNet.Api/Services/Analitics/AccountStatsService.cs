using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Analitics;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Infrastructure.Logging;

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
            .SingleAsync();

        var now = DateTime.UtcNow;

        var accountStats = await _context.AccountsStats
            .Where(a => a.AccountId == accountId)
            .SingleOrDefaultAsync();

        if (accountStats == null)
        {
            var message = "There is no any AccountResume related with target accountId. So it will be created.";
            _logger.LogAccountResumeDoesntExist(message);

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


    private readonly AetherDbContext _context;
    private readonly ILogger<AccountStatsService> _logger;
}