using System;

namespace TipCatDotNet.Api.Models.Analitics;

public readonly struct AccountStatsResponse
{
    public AccountStatsResponse(int id, int transactionsCount, decimal amountPerDay, decimal totalAmount, DateTime currentDate)
    {
        Id = id;
        TransactionsCount = transactionsCount;
        AmountPerDay = amountPerDay;
        TotalAmount = totalAmount;
        CurrentDate = currentDate;
    }


    public int Id { get; }
    public int TransactionsCount { get; }
    public decimal AmountPerDay { get; }
    public decimal TotalAmount { get; }
    public DateTime CurrentDate { get; }
}