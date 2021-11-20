using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services;

public interface IMemberContextService
{
    ValueTask<Result<MemberContext>> Get();
}