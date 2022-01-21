using System;

namespace TipCatDotNet.Api.Data.Analitics;

public class AccountResume
{
    public static AccountResume Empty(in int accountId, in DateTime now)
        => new AccountResume
        {
            AccountId = accountId,
            TransactionsCount = 0,
            AmountPerDay = 0,
            TotalAmount = 0,
            CurrentDate = now,
            Modified = now,
            IsActive = true,
        };


    public static AccountResume Reset(AccountResume accountResume, in DateTime now)
    {
        accountResume.CurrentDate = now;
        accountResume.TransactionsCount = 0;
        accountResume.AmountPerDay = 0;
        accountResume.Modified = now;

        return accountResume;
    }


    public int Id { get; set; }
    public int AccountId { get; set; }
    public int TransactionsCount { get; set; }
    public decimal AmountPerDay { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CurrentDate { get; set; }
    public DateTime Modified { get; set; }
    public bool IsActive { get; set; }
}