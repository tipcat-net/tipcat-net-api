namespace TipCatDotNet.Api.Infrastructure.Auth
{
    public class AzureB2COptions
    {
        public string ClientId { get; set; } = null!;
        public string PolicyId { get; set; } = null!;
        public string TenantId { get; set; } = null!;
    }
}
