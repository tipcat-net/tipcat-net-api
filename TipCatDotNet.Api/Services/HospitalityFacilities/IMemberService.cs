using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IMemberService
    {
        Task<Result<MemberResponse>> AddCurrent(string? identityClaim, MemberPermissions permissions, CancellationToken cancellationToken = default);
        
        Task<Result<MemberResponse>> Get(MemberContext memberContext, int memberId, int accountId, CancellationToken cancellationToken = default);
        
        Task<Result<MemberResponse>> GetCurrent(MemberContext memberContext, CancellationToken cancellationToken = default);

        Task<Result<MemberResponse>> Update(MemberContext memberContext, MemberRequest request, CancellationToken cancellationToken = default);
    }
}
