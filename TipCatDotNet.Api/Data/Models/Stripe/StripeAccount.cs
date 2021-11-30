namespace TipCatDotNet.Api.Data.Models.Stripe
{
    public class StripeAccount
    {
        public string Id { get; set; } = null!;
        public int MemberId { get; set; }
        // Any account info from stripe
    }
}