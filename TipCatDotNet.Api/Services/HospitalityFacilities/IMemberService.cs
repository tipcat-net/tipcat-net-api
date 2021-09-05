using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IMemberService
    {
        Task<Result<MemberInfoResponse>> Add(string? id, MemberPermissions permissions, CancellationToken cancellationToken = default);
    }
}
