using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.HospitalityFacilities
{
    public interface IMemberService
    {
        Task<Result<MemberInfoResponse>> Add(string? id);
    }
}
