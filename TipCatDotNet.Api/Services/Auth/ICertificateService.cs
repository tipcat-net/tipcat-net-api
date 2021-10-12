using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace TipCatDotNet.Api.Services.Auth
{
    public interface ICertificateService
    {
        Task<X509SigningCredentials> BuildSigningCredentials(CancellationToken cancellationToken = default);
    }
}