using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Auth
{
    public interface IInvitationService
    {
        Task<Result> Add(MemberRequest request, CancellationToken cancellationToken = default);
    }
}
