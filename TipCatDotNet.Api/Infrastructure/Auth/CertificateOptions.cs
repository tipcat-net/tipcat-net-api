namespace TipCatDotNet.Api.Infrastructure.Auth
{
    public class CertificateOptions
    {
        public string Name { get; set; } = null!;

        public string Role { get; set; } = null!;

        public string VaultToken { get; set; } = null!;
    }
}
