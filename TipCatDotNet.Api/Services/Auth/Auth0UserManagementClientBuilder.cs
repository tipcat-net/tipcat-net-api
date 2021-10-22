using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.Auth
{
    public class Auth0UserManagementClientBuilder
    {
        public Auth0UserManagementClientBuilder(Auth0ManagementApiOptions options, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _options = options;
        }


        public async Task<ManagementApiClient> Get()
        {
            var tokenRequest = new Auth0TokenRequest(_options.ClientId, _options.ClientSecret);
            var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(tokenRequest));

            using var response = await _httpClient.PostAsync("/oauth/token", content);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await JsonSerializer.DeserializeAsync<Auth0TokenResponse>(await response.Content.ReadAsStreamAsync());

            return new ManagementApiClient(tokenResponse.AccessToken, _options.Domain);
        }
    
        
        private readonly HttpClient _httpClient;
        private readonly Auth0ManagementApiOptions _options;
    }
}
