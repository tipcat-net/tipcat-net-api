using System;

namespace TipCatDotNet.Api.Data.Analitics;

public class AccountResume
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int TransactionsCount { get; set; }
    public decimal AmountPerDay { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CurrentDate { get; set; }
    public bool IsActive { get; set; }
}