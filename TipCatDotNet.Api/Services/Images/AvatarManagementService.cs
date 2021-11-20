using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Images;
using TipCatDotNet.Api.Models.Images.Validators;

namespace TipCatDotNet.Api.Services.Images;

public class AvatarManagementService : IAvatarManagementService
{
    public AvatarManagementService(AetherDbContext context, IAwsImageManagementService awsImageManagementService)
    {
        _awsImageManagementService = awsImageManagementService;
        _context = context;
    }


    public Task<Result<string>> AddOrUpdateMemberAvatar(MemberContext memberContext, MemberAvatarRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(() => _awsImageManagementService.Upload(request.File!, "", cancellationToken))
            .Bind(UpdateMember);


        Result Validate()
        {
            var validator = new MemberAvatarRequestValidator(_context, memberContext);
            return validator.Validate(request, cancellationToken).ToResult();
        }


        async Task<Result<string>> UpdateMember(string avatarUrl)
        {
            return await Task.FromResult(string.Empty);
        }
    }


    private readonly IAwsImageManagementService _awsImageManagementService;
    private readonly AetherDbContext _context;
}
