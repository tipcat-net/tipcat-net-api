using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.VaultClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Invitations;

namespace TipCatDotNet.Api.Services.HospitalityFacilities.Invitations
{
    public class InvitationService : IInvitationService, IDisposable
    {
        public InvitationService(IOptions<AzureB2cOptions> azureB2cOptions, IOptions<CertificateOptions> options, IVaultClient vaultClient)
        {
            _azureB2cOptions = azureB2cOptions.Value;
            _certificateOptions = options.Value;
            _vaultClient = vaultClient;
        }


        public void Dispose()
        {
            _vaultClient.Dispose();
            GC.SuppressFinalize(this);
        }


        // https://github.com/azure-ad-b2c/samples/tree/master/policies/invite#creating-a-signing-certificate
        public async Task<Result<string>> Send(MemberRequest request)
        {
            var invitationLink = await BuildInvitationLink(request);

            return Result.Success(invitationLink);
        }


        private string BuildIdToken(in MemberRequest request, in X509SigningCredentials signingCredentials)
        {
            var now = DateTime.UtcNow;

            var issuer = string.Empty;
            var claims = new List<Claim>
            {
                new("name", $"{request.FirstName} {request.LastName}", ClaimValueTypes.String, issuer),
                new("email", request.Email!, ClaimValueTypes.String, issuer)
            };
            
            var token = new JwtSecurityToken(issuer, _azureB2cOptions.ClientId, claims, now, now.AddDays(7), signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.WriteToken(token);
        }


        private async Task<string> BuildInvitationLink(MemberRequest request)
        {
            var signingCredentials = await BuildSigningCredentials();
            var idToken = BuildIdToken(in request, in signingCredentials);
            var nonce = Guid.NewGuid().ToString("N");

            return string.Format(InvitationUrlTemplate, _azureB2cOptions.TenantId, _azureB2cOptions.PolicyId, _azureB2cOptions.ClientId,
                Uri.EscapeDataString(RedirectUri), nonce, idToken);
        }


        private async Task<X509SigningCredentials> BuildSigningCredentials()
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


        public const string InvitationUrlTemplate =
            "https://{0}.b2clogin.com/{0}.onmicrosoft.com/{1}/oauth2/v2.0/authorize?client_id={2}&nonce={4}&redirect_uri={3}&scope=openid&response_type=id_token&id_token_hint={5}";
        public const string RedirectUri = "https://dev.tipcat.net/";

        private readonly CertificateOptions _certificateOptions;
        private readonly AzureB2cOptions _azureB2cOptions;
        private readonly IVaultClient _vaultClient;
    }
}
