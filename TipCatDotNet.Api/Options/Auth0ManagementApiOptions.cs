using System;

namespace TipCatDotNet.Api.Options
{
    public class Auth0ManagementApiOptions
    {
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string ConnectionId { get; set; } = null!;
        public Uri Domain { get; set; } = null!;
    }
}
