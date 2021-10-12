using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure.Auth;
using TipCatDotNet.Api.Data.Models.Auth;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.Auth
{
    public class InvitationService : IInvitationService
    {
        public InvitationService(IOptions<AzureB2COptions> azureB2COptions, IOptions<InvitationOptions> invitationOptions, AetherDbContext context, ICertificateService certificateService)
        {
            _azureB2COptions = azureB2COptions.Value;
            _certificateService = certificateService;
            _context = context;
            _invitationOptions = invitationOptions.Value;
        }


        public async Task<Result> Add(MemberRequest request, CancellationToken cancellationToken = default)
        {
            return await Result.Success()
                .Map(async () => await BuildInvitationLink(request, cancellationToken))
                .Map(link => (link, BuildInvitationCode(request.Id!.Value)))
                .Bind(AddInternal);


            async Task<Result> AddInternal((string, string) invitationData)
            {
                var (link, code) = invitationData;

                var newInvitation = new MemberInvitation
                {
                    Code = code,
                    Link = link,
                    MemberId = request.Id!.Value,
                    State = InvitationStates.NotSent
                };

                _context.MemberInvitations.Add(newInvitation);
                await _context.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
        }


        public async Task<Result<int>> Redeem(string code, CancellationToken cancellationToken = default)
        {
            var invitation = await _context.MemberInvitations
                .Where(i => i.Code == code)
                .SingleOrDefaultAsync(cancellationToken);
            
            return await AcceptInternal(invitation, cancellationToken);
        }


        public async Task<Result> Send(int memberId, CancellationToken cancellationToken = default)
        {
            var invitation = await _context.MemberInvitations
                .Where(i => i.MemberId == memberId && i.State == InvitationStates.NotSent)
                .SingleOrDefaultAsync(cancellationToken);

            if (invitation is null)
                return Result.Failure("Can't find an invitation for a member with ID");

            // TODO: implement actual sending

            /*invitation.State = InvitationStates.Sent;
            _context.MemberInvitations.Update(invitation);
            await _context.SaveChangesAsync(cancellationToken);*/

            return Result.Success();
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


        private static string BuildInvitationCode(int memberId)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(memberId.ToString());
            return Convert.ToBase64String(bytes);
        }


        // https://github.com/azure-ad-b2c/samples/tree/master/policies/invite#creating-a-signing-certificate
        private async Task<string> BuildInvitationLink(MemberRequest request, CancellationToken cancellationToken)
        {
            var signingCredentials = await _certificateService.BuildSigningCredentials(cancellationToken);
            var idToken = BuildIdToken(in request, in signingCredentials);
            var nonce = Guid.NewGuid().ToString("N");

            return string.Format(_invitationOptions.UrlTemplate, _azureB2COptions.TenantId, _azureB2COptions.PolicyId, _azureB2COptions.ClientId,
                Uri.EscapeDataString(_invitationOptions.ReturnUrl), nonce, idToken);
        }


        private async Task<Result<int>> AcceptInternal(MemberInvitation? invitation, CancellationToken cancellationToken)
        {
            if (invitation is null)
                return Result.Failure<int>("Can't find an invitation for a member with ID");

            if (invitation.State == InvitationStates.Accepted)
                return Result.Failure<int>("An invitation has been accepted already.");
            
            if (invitation.State < InvitationStates.Sent)
                return Result.Failure<int>("An invitation has been accepted already.");

            invitation.State = InvitationStates.Accepted;
            _context.MemberInvitations.Update(invitation);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(invitation.MemberId);
        }


        private readonly AzureB2COptions _azureB2COptions;
        private readonly ICertificateService _certificateService;
        private readonly AetherDbContext _context;
        private readonly InvitationOptions _invitationOptions;
    }
}
