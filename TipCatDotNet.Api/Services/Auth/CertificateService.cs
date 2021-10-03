using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TipCatDotNet.Api.Infrastructure.Auth;

namespace TipCatDotNet.Api.Services.Auth
{
    public class CertificateService : ICertificateService, IDisposable
    {
        public CertificateService(IOptions<CertificateOptions> certificateOptions, IVaultClient vaultClient)
        {
            _certificateOptions = certificateOptions.Value;
            _vaultClient = vaultClient;
        }


        // TODO: add a cancellation token support for the vault client
        public async Task<X509SigningCredentials> BuildSigningCredentials(CancellationToken cancellationToken = default)
        {
            await _vaultClient.Login(_certificateOptions.VaultToken, LoginMethods.Token);
            var (certificateString, privateKeyString) = await _vaultClient.IssueCertificate(_certificateOptions.Role, _certificateOptions.Name);

            var certificate = CreateCertificate(certificateString, privateKeyString);
            return new X509SigningCredentials(certificate, "RS256");


            static X509Certificate2 CreateCertificate(string certificate, string privateKey)
            {
                var publicCert = new X509Certificate2(Encoding.ASCII.GetBytes(certificate));
                var privateKeyProvider = PrivateKeyExtractor.GetProviderFromKey(privateKey);
                
                return publicCert.CopyWithPrivateKey(privateKeyProvider);
            }
        }


        public void Dispose()
        {
            _vaultClient.Dispose();
            GC.SuppressFinalize(this);
        }


        private readonly CertificateOptions _certificateOptions;
        private readonly IVaultClient _vaultClient;
    }
}
