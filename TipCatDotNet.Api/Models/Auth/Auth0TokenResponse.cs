using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Auth;

public readonly struct Auth0TokenResponse
{
    [JsonConstructor]
    public Auth0TokenResponse(string accessToken)
    {
        AccessToken = accessToken;
    }


    [JsonPropertyName("access_token")]
    public string AccessToken { get; }
}