using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IFacilityService
    {
        Task<Result<int>> AddDefault(int accountId, CancellationToken cancellationToken = default);
        Task<Result<int>> TransferMember(int memberId, int facilityId, CancellationToken cancellationToken = default);
    }
}