using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Auth
{
    public interface IInvitationService
    {
        Task<Result> Send(MemberRequest request, CancellationToken cancellationToken = default);

        Task<Result> Redeem(int memberId, CancellationToken cancellationToken = default);

        Task<Result> Resend(int memberId, CancellationToken cancellationToken = default);
    }
}
