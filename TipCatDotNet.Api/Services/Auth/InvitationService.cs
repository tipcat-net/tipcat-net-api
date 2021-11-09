using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.MailSender;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Auth;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.Auth.Validators;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Validators;
using TipCatDotNet.Api.Models.Mailing;
using TipCatDotNet.Api.Options;
using TipCatDotNet.Api.Services.Company;

namespace TipCatDotNet.Api.Services.Auth
{
    public class InvitationService : IInvitationService
    {
        public InvitationService(IOptionsMonitor<InvitationServiceOptions> options, AetherDbContext context, IMailSender mailSender,
            IUserManagementClient userManagementClient)
        {
            _context = context;
            _mailSender = mailSender;
            _options = options.CurrentValue;
            _userManagementClient = userManagementClient;
        }


        public Task<Result> CreateAndSend(MemberRequest request, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Bind(CreateRequest)
                .Bind(AddOnAuthProvider)
                .Bind(invitation => SendInternal(request, invitation, cancellationToken));


            async Task<Result<MemberInvitation>> CreateRequest()
            {
                var now = DateTime.UtcNow;
                var invitation = new MemberInvitation
                {
                    MemberId = request.Id!.Value,
                    State = InvitationStates.NotSent,
                    Created = now,
                    Modified = now
                };

                _context.MemberInvitations.Add(invitation);

                await _context.SaveChangesAsync(cancellationToken);
                //_context.DetachEntities();

                return invitation;
            }


            async Task<Result<MemberInvitation>> AddOnAuthProvider(MemberInvitation invitation)
            {
                var result = await _userManagementClient.Add(request, isEmailVerified: true, cancellationToken);
                if (result.IsFailure)
                    return Result.Failure<MemberInvitation>(result.Error);

                return invitation;
            }
        }


        public async Task<InvitationStates> GetState(int memberId, CancellationToken cancellationToken = default)
            => await _context.MemberInvitations
                .Where(i => i.MemberId == memberId)
                .Select(i => i.State)
                .SingleOrDefaultAsync(cancellationToken);


        public async Task<Dictionary<int, InvitationStates>> GetState(IEnumerable<int> memberIds, CancellationToken cancellationToken = default)
            => await _context.Members
                .Where(m => memberIds.Contains(m.Id))
                .Join(_context.MemberInvitations, m => m.Id, i => i.MemberId, (m, i) => new { m.Id, i.State })
                .ToDictionaryAsync(x => x.Id, x => x.State, cancellationToken);


        public async Task<Result> Redeem(int memberId, CancellationToken cancellationToken = default)
        {
            var invitation = await _context.MemberInvitations
                .SingleOrDefaultAsync(i => i.MemberId == memberId, cancellationToken);

            if (invitation is null)
                return Result.Failure("The invitation was non found.");

            invitation.State = InvitationStates.Accepted;
            invitation.Modified = DateTime.UtcNow;
            _context.MemberInvitations.Update(invitation);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        public Task<Result> Send(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default)
        {
            return ValidateAccess()
                .Bind(ValidateInvitation)
                .Bind(EnrichRequest)
                .Bind(data => SendInternal(data.Request, data.Invitation, cancellationToken));


            Result ValidateAccess()
            {
                var validator = new MemberRequestValidator(memberContext, _context);
                return validator.ValidateInvite(request).ToResult();
            }


            async Task<Result<MemberInvitation>> ValidateInvitation()
            {
                var existedInvitation = await _context.MemberInvitations
                    .Where(i => i.MemberId == request.Id!)
                    .SingleOrDefaultAsync(cancellationToken);

                var validator = new MemberInvitationValidator();
                return validator.Validate(existedInvitation).ToResult(existedInvitation);
            }


            async Task<Result<(MemberInvitation Invitation, MemberRequest Request)>> EnrichRequest(MemberInvitation invitation)
            {
                var updatedRequest = await _context.Members
                    .Where(m => m.Id == request.Id!)
                    .Select(m => new MemberRequest(m.Id, m.AccountId, m.FirstName, m.LastName, m.Email, m.Permissions))
                    .SingleAsync(cancellationToken);

                return (invitation, updatedRequest);
            }
        }

        
        private Task<Result> SendInternal(MemberRequest request, MemberInvitation memberInvitation, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return ChangePasswordIfNeeded()
                .Bind(AddLinkAndMarkAsSent)
                .Bind(ResendEmail);


            async Task<Result<MemberInvitation>> ChangePasswordIfNeeded()
            {
                if (memberInvitation.Link is not null && now.AddDays(-7) <= memberInvitation.Created)
                    return memberInvitation;

                // A password change ticket lifetime is 7 days, so we have to reestablish it to continue
                var result = await _userManagementClient.ChangePassword(request.Email!, cancellationToken);
                if (result.IsFailure)
                    return Result.Failure<MemberInvitation>(result.Error);

                memberInvitation.Created = now;
                memberInvitation.Link = result.Value;

                return memberInvitation;
            }


            async Task<Result<MemberInvitation>> AddLinkAndMarkAsSent(MemberInvitation invitation)
            {
                invitation.State = InvitationStates.Sent;
                invitation.Modified = DateTime.UtcNow;
                _context.MemberInvitations.Update(invitation);

                await _context.SaveChangesAsync(cancellationToken);

                return invitation;
            }


            async Task<Result> ResendEmail(MemberInvitation invitation)
            {
                var accountName = await _context.Accounts
                    .Where(a => a.Id == request.AccountId)
                    .Select(a => a.OperatingName)
                    .SingleOrDefaultAsync(cancellationToken);

                return await _mailSender.Send(_options.TemplateId, request.Email!,
                    new MemberInvitationEmail(accountName, invitation.Link!, CompanyInfoService.Get));
            }
        }


        private readonly AetherDbContext _context;
        private readonly IMailSender _mailSender;
        private readonly InvitationServiceOptions _options;
        private readonly IUserManagementClient _userManagementClient;
    }
}
