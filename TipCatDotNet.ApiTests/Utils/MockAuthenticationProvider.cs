using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace TipCatDotNet.ApiTests.Utils
{
    public class MockAuthenticationProvider : IAuthenticationProvider
    {
        public Task AuthenticateRequestAsync(HttpRequestMessage request) 
            => Task.CompletedTask;
    }
}
