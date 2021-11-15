namespace TipCatDotNet.Api.Options
{
    public class StripeOptions
    {
        public string PublishableKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string WebhookSecret { get; set; } = null!;
    }
}