using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Preferences;

namespace TipCatDotNet.Api.Services.Preferences;

public interface IPreferencesService
{
    Task<Result<PreferencesResponse>> AddOrUpdate(MemberContext memberContext, PreferencesRequest request, CancellationToken cancellationToken = default);

    Task<PreferencesResponse> Get(MemberContext memberContext, CancellationToken cancellationToken = default);
}