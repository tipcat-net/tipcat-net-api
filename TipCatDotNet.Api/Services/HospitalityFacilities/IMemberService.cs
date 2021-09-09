using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IMemberService
    {
        Task<Result<MemberInfoResponse>> AddCurrent(string? tokenId, MemberPermissions permissions, CancellationToken cancellationToken = default);
        Task<Result<MemberInfoResponse>> GetCurrent(MemberContext memberContext, CancellationToken cancellationToken = default);
        Task<Result<MemberInfoResponse>> UpdateCurrent(string? id,MemberUpdateRequest request, CancellationToken cancellationToken = default);
        Task<Result<MemberAvatarResponse>> UpdateAvatar(string? id, MemberAvatarRequest request, CancellationToken cancellationToken = default);
    }
}
