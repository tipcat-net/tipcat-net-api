namespace TipCatDotNet.Api.Options
{
    public class Auth0ManagementApiOptions
    {
        public string Audience { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string ConnectionId { get; set; } = null!;
    }
}
