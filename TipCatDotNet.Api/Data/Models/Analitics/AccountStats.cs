using System;

namespace TipCatDotNet.Api.Data.Analitics;

public class AccountStats
{
    public static AccountStats Empty(in int accountId, string currency, in DateTime now)
        => new AccountStats
        {
            AccountId = accountId,
            TransactionsCount = 0,
            AmountPerDay = 0,
            TotalAmount = 0,
            Currency = currency,
            CurrentDate = now,
            Modified = now,
            IsActive = true,
        };


    public static AccountStats Reset(AccountStats accountStats, in DateTime now)
    {
        accountStats.CurrentDate = now;
        accountStats.TransactionsCount = 0;
        accountStats.AmountPerDay = 0;
        accountStats.Modified = now;

        return accountStats;
    }


    public int Id { get; set; }
    public int AccountId { get; set; }
    public int TransactionsCount { get; set; }
    public decimal AmountPerDay { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime CurrentDate { get; set; }
    public DateTime Modified { get; set; }
    public bool IsActive { get; set; }
}