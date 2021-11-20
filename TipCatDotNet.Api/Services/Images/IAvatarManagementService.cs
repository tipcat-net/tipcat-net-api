using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Images;

namespace TipCatDotNet.Api.Services.Images;

public interface IAvatarManagementService
{
    Task<Result<string>> AddOrUpdateMemberAvatar(MemberContext memberContext, MemberAvatarRequest request, CancellationToken cancellationToken = default);
}