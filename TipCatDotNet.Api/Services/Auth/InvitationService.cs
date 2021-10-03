using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TipCatDotNet.Api.Infrastructure.Auth;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Auth
{
    public class InvitationService : IInvitationService
    {
        public InvitationService(IOptions<AzureB2COptions> azureB2COptions, IOptions<InvitationOptions> invitationOptions, ICertificateService certificateService)
        {
            _azureB2COptions = azureB2COptions.Value;
            _certificateService = certificateService;
            _invitationOptions = invitationOptions.Value;
        }


        // https://github.com/azure-ad-b2c/samples/tree/master/policies/invite#creating-a-signing-certificate
        public async Task<Result<string>> Send(MemberRequest request, CancellationToken cancellationToken = default)
        {
            var invitationLink = await BuildInvitationLink(request, cancellationToken);

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
            
            var token = new JwtSecurityToken(issuer, _azureB2COptions.ClientId, claims, now, now.AddDays(7), signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.WriteToken(token);
        }


        private async Task<string> BuildInvitationLink(MemberRequest request, CancellationToken cancellationToken)
        {
            var signingCredentials = await _certificateService.BuildSigningCredentials(cancellationToken);
            var idToken = BuildIdToken(in request, in signingCredentials);
            var nonce = Guid.NewGuid().ToString("N");

            return string.Format(_invitationOptions.UrlTemplate, _azureB2COptions.TenantId, _azureB2COptions.PolicyId, _azureB2COptions.ClientId,
                Uri.EscapeDataString(_invitationOptions.ReturnUrl), nonce, idToken);
        }


        private readonly AzureB2COptions _azureB2COptions;
        private readonly InvitationOptions _invitationOptions;
        private readonly ICertificateService _certificateService;
    }
}
