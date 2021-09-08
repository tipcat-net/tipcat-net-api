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


        public Task<User> GetUser(string id, CancellationToken cancellationToken)
        {
            return _client.Users[id]
                .Request()
                .Select(u => new { u.GivenName, u.Surname, u.Identities }) // TODO: implement a mock for IUserRequest
                .GetAsync(cancellationToken);
        }


        private readonly GraphServiceClient _client;
    }
}
