namespace TipCatDotNet.Api.Data.Models.Stripe;

public class StripeAccount
{
    public int Id { get; set; }
    public string StripeId { get; set; } = null!;
    public int MemberId { get; set; }
    // Any account info from stripe
}