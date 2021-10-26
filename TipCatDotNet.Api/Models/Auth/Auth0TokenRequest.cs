using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Auth
{
    public readonly struct Auth0TokenRequest
    {
        public Auth0TokenRequest(string clientId, string clientSecret, string audience, string grantType  = "client_credentials")
        {
            Audience = audience;
            GrantType = grantType;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }


        [JsonPropertyName("audience")]
        public string Audience { get; }
        [JsonPropertyName("client_id")]
        public string ClientId { get; }
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; }
        [JsonPropertyName("grant_type")]
        public string GrantType { get; }
    }
}
