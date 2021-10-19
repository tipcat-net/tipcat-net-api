using System;
using System.Threading;
using System.Threading.Tasks;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Models.Auth;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.Auth
{
    public class Auth0UserManagementClient : IUserManagementClient
    {
        public Auth0UserManagementClient(IOptionsMonitor<Auth0ManagementApiOptions> options)
        {
            _options = options.CurrentValue;
            _client = new ManagementApiClient(_options.Token, _options.Domain);
        }


        // https://auth0.com/docs/brand-and-customize/email/send-email-invitations-for-application-signup#create-password-change-tickets
        public Task<Result<string>> Add(MemberRequest request, CancellationToken cancellationToken)
            => ExecuteRequest(async () =>
                {
                    var connection = await _client.Connections.GetAsync(_options.ConnectionId, "name", cancellationToken: cancellationToken);

                    return await _client.Users.CreateAsync(new UserCreateRequest
                    {
                        Connection = connection.Name,
                        Email = request.Email,
                        EmailVerified = false,
                        FirstName = request.FirstName,
                        LastName = request.LastName
                    }, cancellationToken);
                })
                .Map(user => user.UserId);


        public Task<Result> ChangePassword(string email, CancellationToken cancellationToken)
            => ExecuteRequest(() => _client.Tickets.CreatePasswordChangeTicketAsync(new PasswordChangeTicketRequest
                {
                    ConnectionId = _options.ConnectionId,
                    Email = email,
                    MarkEmailAsVerified = true,
                    Ttl = 60 * 60 * 24 * 7
                }, cancellationToken))
                .Bind(_ => Result.Success());


        public async Task<Result<UserContext>> Get(string identityClaim, CancellationToken cancellationToken)
            => await ExecuteRequest(() => _client.Users.GetAsync(identityClaim, "email,given_name,family_name", cancellationToken: cancellationToken))
                .Ensure(user => user is not null, "The user isn't found on an auth provider.")
                .Map(user => new UserContext(user.FirstName, user.LastName, user.Email));


        private static async Task<Result<T>> ExecuteRequest<T>(Func<Task<T>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception ex)
            {
                // We omit exceptions in this class to integrate it to our code flow.
                return Result.Failure<T>($"Auth provider error: {ex.Message}");
            }
        }


        private readonly ManagementApiClient _client;
        private readonly Auth0ManagementApiOptions _options;
    }
}
