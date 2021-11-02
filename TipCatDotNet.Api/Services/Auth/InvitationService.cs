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
using TipCatDotNet.Api.Models.HospitalityFacilities;
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


        public Task<Result> Send(MemberRequest request, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Tap(CreateRequest)
                .Bind(() => _userManagementClient.Add(request, isEmailVerified: true, cancellationToken))
                .Bind(_ => _userManagementClient.ChangePassword(request.Email!, cancellationToken))
                .Bind(AddLinkAndMarkAsSent)
                .Bind(() => Resend(request.Id!.Value, cancellationToken));


            async Task CreateRequest()
            {
                var memberId = request.Id!.Value;
                var invitation = await _context.MemberInvitations
                    .SingleOrDefaultAsync(i => i.MemberId == memberId, cancellationToken);

                if (invitation is null)
                {
                    _context.MemberInvitations.Add(new MemberInvitation
                    {
                        MemberId = memberId,
                        State = InvitationStates.NotSent
                    });
                }
                else
                {
                    invitation.State = InvitationStates.NotSent;
                    _context.MemberInvitations.Update(invitation);
                }

                await _context.SaveChangesAsync(cancellationToken);
                _context.DetachEntities();
            }


            async Task<Result> AddLinkAndMarkAsSent(string invitationLink)
            {
                var invitation = await _context.MemberInvitations
                    .Where(i => i.MemberId == request.Id)
                    .SingleOrDefaultAsync(cancellationToken);

                invitation.State = InvitationStates.Sent;
                invitation.Link = invitationLink;
                _context.MemberInvitations.Update(invitation);

                await _context.SaveChangesAsync(cancellationToken);
                _context.DetachEntities();

                return Result.Success();
            }
        }


        public async Task<Result> Redeem(int memberId, CancellationToken cancellationToken = default)
        {
            var invitation = await _context.MemberInvitations
                .SingleOrDefaultAsync(i => i.MemberId == memberId, cancellationToken);

            if (invitation is null)
                return Result.Failure("The invitation was non found.");

            invitation.State = InvitationStates.Accepted;
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }


        public async Task<Result> Resend(int memberId, CancellationToken cancellationToken = default)
        {
            var link = await _context.MemberInvitations
                .Where(i => i.MemberId == memberId && (i.State == InvitationStates.NotSent || i.State == InvitationStates.Sent))
                .Select(i => i.Link)
                .SingleOrDefaultAsync(cancellationToken);

            if (link is null)
                return Result.Failure($"No invitations found for a member {memberId}.");

            var member = await _context.Members
                .Where(m => m.Id == memberId)
                .Select(m => new { m.Email, m.AccountId })
                .SingleOrDefaultAsync(cancellationToken);

            if (member is null)
                return Result.Failure($"No email addresses found for a member {memberId}.");

            var accountName = await _context.Accounts
                .Where(a => a.Id == member.AccountId)
                .Select(a => a.OperatingName)
                .SingleOrDefaultAsync(cancellationToken);

            return await _mailSender.Send(_options.TemplateId, member.Email!,
                new MemberInvitationEmail(accountName, link, CompanyInfoService.Get));
        }


        private readonly AetherDbContext _context;
        private readonly IMailSender _mailSender;
        private readonly InvitationServiceOptions _options;
        private readonly IUserManagementClient _userManagementClient;
    }
}
