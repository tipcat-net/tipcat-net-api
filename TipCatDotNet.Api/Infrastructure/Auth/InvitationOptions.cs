namespace TipCatDotNet.Api.Infrastructure.Auth
{
    public class InvitationOptions
    {
        public string ReturnUrl { get; set; } = null!;
        public string UrlTemplate { get; set; } = null!;
    }
}
