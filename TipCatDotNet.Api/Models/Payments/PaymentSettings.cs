namespace TipCatDotNet.Api.Models.Payments
{
    public class PaymentSettings
    {
        public string StripePublicKey { get; set; } = null!;
        public string StripePrivateKey { get; set; } = null!;
    }
}