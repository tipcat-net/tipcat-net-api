using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IFacilityService
    {
        Task<Result<FacilityResponse>> Add(MemberContext memberContext, FacilityRequest request, CancellationToken cancellationToken = default);
        Task<Result<int>> AddDefault(int accountId, CancellationToken cancellationToken = default);
        Task<List<FacilityResponse>> Get(int accountId, CancellationToken cancellationToken = default);
        Task<Result<int>> TransferMember(int memberId, int facilityId, CancellationToken cancellationToken = default);
        Task<Result<FacilityResponse>> Update(MemberContext memberContext, FacilityRequest request, CancellationToken cancellationToken = default);
    }
}