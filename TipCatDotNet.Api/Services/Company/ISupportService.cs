using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.Company;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Company;

public interface ISupportService
{
    Task<Result> SendRequest(MemberContext memberContext, SupportRequest request, CancellationToken cancellationToken = default);
}