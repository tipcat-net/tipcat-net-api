using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Data.Models.Auth;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.Auth.Enums;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Auth
{
    public class InvitationService : IInvitationService
    {
        public InvitationService(AetherDbContext context, IUserManagementClient userManagementClient)
        {
            _context = context;
            _userManagementClient = userManagementClient;
        }


        public Task<Result> Send(MemberRequest request, CancellationToken cancellationToken = default)
        {
            return Result.Success()
                .Tap(LogRequest)
                .Bind(() => _userManagementClient.Add(request, cancellationToken))
                .Bind(_ => _userManagementClient.ChangePassword(request.Email!, cancellationToken))
                .Tap(LogPasswordResetMessageSent);


            async Task LogRequest()
            {
                _context.MemberInvitations.Add(new MemberInvitation
                {
                    MemberId = request.Id!.Value,
                    State = InvitationStates.NotSent
                });

                await _context.SaveChangesAsync(cancellationToken);
                _context.DetachEntities();
            }


            async Task LogPasswordResetMessageSent()
            {
                var invitation = await _context.MemberInvitations
                    .Where(i => i.MemberId == request.Id)
                    .SingleOrDefaultAsync(cancellationToken);

                invitation.State = InvitationStates.Sent;
                _context.MemberInvitations.Update(invitation);

                await _context.SaveChangesAsync(cancellationToken);
            }
        }


        private readonly AetherDbContext _context;
        private readonly IUserManagementClient _userManagementClient;
    }
}
