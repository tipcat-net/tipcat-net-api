using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities.Invitations
{
    public interface IInvitationService
    {
        Task<Result<string>> Send(MemberRequest request);
    }
}
