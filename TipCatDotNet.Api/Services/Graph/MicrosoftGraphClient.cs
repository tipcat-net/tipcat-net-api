using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TipCatDotNet.Api.Services.Graph
{
    public class MicrosoftGraphClient : IMicrosoftGraphClient
    {
        public MicrosoftGraphClient(GraphServiceClient client)
        {
            _client = client;
        }


        public Task<Invitation> InviteMember(string email, CancellationToken cancellationToken)
        {
            var invitation = new Invitation
            {
                InvitedUserEmailAddress = email,
                InviteRedirectUrl = "https://dev.tipcat.net",
                SendInvitationMessage = true
            };

            return _client.Invitations
                .Request()
                .AddAsync(invitation, cancellationToken);
        }


        public Task<User> GetUser(string id, CancellationToken cancellationToken)
            => _client.Users[id]
                .Request()
                .Select(u => new { u.GivenName, u.Surname, u.Identities })
                .GetAsync(cancellationToken);


        private readonly GraphServiceClient _client;
    }
}
