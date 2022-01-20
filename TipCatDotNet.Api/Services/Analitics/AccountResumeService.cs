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

public class AccountResumeService : IAccountResumeService
{
    public AccountResumeService(AetherDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger<AccountResumeService>();
    }


    public async Task<Result> CreateOrUpdate(Transaction transaction, CancellationToken cancellationToken = default)
    {
        var accountId = await _context.Facilities
        .Where(m => m.Id == transaction.FacilityId)
        .Select(m => m.AccountId)
        .SingleOrDefaultAsync();

        var now = DateTime.UtcNow;

        var accountResume = await _context.AccountResumes
            .Where(a => a.AccountId == accountId)
            .SingleOrDefaultAsync();

        if (accountResume == null)
        {
            var message = "There is no any AccountResume related with target accountId. So it will be created.";
            _logger.LogAccountResumeDoesntExistException(message);

            accountResume = new AccountResume
            {
                AccountId = accountId,
                TransactionsCount = 0,
                AmountPerDay = 0,
                TotalAmount = 0,
                CurrentDate = now,
                IsActive = true,
            };
        }

        accountResume.TransactionsCount += 1;
        accountResume.AmountPerDay += transaction.Amount;
        accountResume.TotalAmount += transaction.Amount;

        _context.AccountResumes.Update(accountResume);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    private readonly AetherDbContext _context;
    private readonly ILogger<AccountResumeService> _logger;
}