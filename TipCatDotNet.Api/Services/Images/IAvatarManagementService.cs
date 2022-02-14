using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Services.Images;

public interface IAvatarManagementService<in T>
{
    Task<Result<string>> AddOrUpdate(MemberContext memberContext, T request, CancellationToken cancellationToken = default);
    Task<Result> Remove(MemberContext memberContext, T request, CancellationToken cancellationToken = default);
    Task<Result<string>> UseParent(MemberContext memberContext, T request, CancellationToken cancellationToken = default);
}