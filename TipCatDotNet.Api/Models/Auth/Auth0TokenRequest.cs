using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Auth
{
    public readonly struct Auth0TokenRequest
    {
        public Auth0TokenRequest(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }


        [JsonPropertyName("client_id")]
        public string ClientId { get; }
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; }
    }
}
