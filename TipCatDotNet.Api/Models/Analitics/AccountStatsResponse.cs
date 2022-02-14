using System;
using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.Analitics;

public readonly struct AccountStatsResponse
{
    public AccountStatsResponse(int id, int transactionsCount, decimal amountPerDay,
        decimal totalAmount, string currency, DateTime currentDate, List<FacilityStatsResponse>? facilities)
    {
        Id = id;
        TransactionsCount = transactionsCount;
        AmountPerDay = amountPerDay;
        TotalAmount = totalAmount;
        Currency = currency;
        CurrentDate = currentDate;
        Facilities = facilities;
    }


    public int Id { get; }
    public int TransactionsCount { get; }
    public decimal AmountPerDay { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; } = null!;
    public DateTime CurrentDate { get; }
    public List<FacilityStatsResponse>? Facilities { get; } = null!;
}