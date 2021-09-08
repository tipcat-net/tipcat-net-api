using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TipCatDotNet.Api.Services.Graph
{
    public interface IMicrosoftGraphClient
    {
        Task<User> GetUser(string id, CancellationToken cancellationToken);
    }
}
