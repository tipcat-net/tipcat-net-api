using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IFacilityService
    {
        Task<Result<int>> AddDefaultFacility(int accountId, CancellationToken cancellationToken = default);
        Task<Result<int>> TransferMemberToFacility(int memberId, int facilityId, CancellationToken cancellationToken = default);
    }
}