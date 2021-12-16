using System;

namespace TipCatDotNet.Api.Data.Models.Stripe;

public class StripeAccount
{
    public int Id { get; set; }
    public string StripeId { get; set; } = null!;
    public int MemberId { get; set; }
    public DateTime LastPaidOut { get; set; }
    public DateTime LastReceived { get; set; }
}