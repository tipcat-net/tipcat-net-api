using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Infrastructure.Logging;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.Auth
{
    public class Auth0UserManagementClient : IUserManagementClient
    {
        public Auth0UserManagementClient(ILoggerFactory loggerFactory, IOptionsMonitor<Auth0ManagementApiOptions> options, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger<Auth0UserManagementClient>();
            _options = options.CurrentValue;
        }


        // https://auth0.com/docs/brand-and-customize/email/send-email-invitations-for-application-signup#create-password-change-tickets
        public Task<Result<string>> Add(MemberRequest request, bool isEmailVerified, CancellationToken cancellationToken)
            => ExecuteRequest(async client =>
                {
                    var connection = await client.Connections.GetAsync(_options.ConnectionId, "name", cancellationToken: cancellationToken);

                    var random = new byte[20];
                    var generator = RandomNumberGenerator.Create();
                    generator.GetBytes(random);

                    return await client.Users.CreateAsync(new UserCreateRequest
                    {
                        Connection = connection.Name,
                        Email = request.Email,
                        EmailVerified = isEmailVerified,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Password = random.ToString()
                    }, cancellationToken);
                }, _options, _httpClient, _logger)
                .Map(user => user.UserId);


        public Task<Result<string>> ChangePassword(string email, CancellationToken cancellationToken)
            => ExecuteRequest(client => client.Tickets.CreatePasswordChangeTicketAsync(new PasswordChangeTicketRequest
                {
                    ConnectionId = _options.ConnectionId,
                    Email = email,
                    MarkEmailAsVerified = true,
                    ResultUrl = _options.RedirectUrl,
                    Ttl = 60 * 60 * 24 * 7 // 7 days
                }, cancellationToken), _options, _httpClient, _logger)
                .Bind(ticket => Result.Success(ticket.Value));


        public Task<Result<UserContext>> Get(string identityClaim, CancellationToken cancellationToken)
            => ExecuteRequest(client => client.Users.GetAsync(identityClaim, "email,given_name,family_name", 
                    cancellationToken: cancellationToken), _options, _httpClient, _logger)
                .Ensure(user => user is not null, "The user isn't found on an auth provider.")
                .Map(user => new UserContext(user.FirstName, user.LastName, user.Email));


        private static async Task<Result<T>> ExecuteRequest<T>(Func<ManagementApiClient, Task<T>> func, Auth0ManagementApiOptions options, HttpClient httpClient, ILogger logger)
        {
            try
            {
                using var client = await GetManagementApiClient();
                return await func(client);
            }
            catch (Exception ex)
            {
                // We omit exceptions in this class to integrate it to our code flow.
                logger.LogAuth0Exception(ex.Message);
                return Result.Failure<T>($"Auth provider error: {ex.Message}");
            }


            async Task<ManagementApiClient> GetManagementApiClient()
            {
                var tokenRequest = new Auth0TokenRequest(options.ClientId, options.ClientSecret, options.Audience);
                var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(tokenRequest));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using var response = await httpClient.PostAsync("/oauth/token", content);

                response.EnsureSuccessStatusCode();
                var tokenResponse = await JsonSerializer.DeserializeAsync<Auth0TokenResponse>(await response.Content.ReadAsStreamAsync());

                return new ManagementApiClient(tokenResponse.AccessToken, new Uri(options.Audience));
            }
        }


        private readonly HttpClient _httpClient;
        private readonly ILogger<Auth0UserManagementClient> _logger;
        private readonly Auth0ManagementApiOptions _options;
    }
}

