using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Infrastructure;
using TipCatDotNet.Api.Models.HospitalityFacilities;
using TipCatDotNet.Api.Models.Images;
using TipCatDotNet.Api.Models.Images.Validators;
using TipCatDotNet.Api.Options;

namespace TipCatDotNet.Api.Services.Images;

public class FacilityAvatarManagementService : IAvatarManagementService<FacilityAvatarRequest>
{
    public FacilityAvatarManagementService(IOptionsMonitor<AvatarManagementServiceOptions> options, AetherDbContext context,
        IAwsImageManagementService awsImageManagementService)
    {
        _awsImageManagementService = awsImageManagementService;
        _context = context;
        _options = options.CurrentValue;
    }


    public Task<Result<string>> AddOrUpdate(MemberContext memberContext, FacilityAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyManagementService.BuildFacilityKey(request.AccountId, request.FacilityId);

        return Validate()
            .Bind(() => _awsImageManagementService.Upload(_options.BucketName, request.File!, key, cancellationToken))
            .Bind(UpdateMember);


        Result Validate()
        {
            var validator = new FacilityAvatarRequestValidator(_context, memberContext);
            return validator.ValidateAdd(request, cancellationToken).ToResult();
        }


        async Task<Result<string>> UpdateMember(string avatarUrl)
        {
            var facility = await _context.Facilities
                .SingleAsync(m => m.Id == request.FacilityId, cancellationToken);

            facility.AvatarUrl = avatarUrl;
            _context.Facilities.Update(facility);

            await _context.SaveChangesAsync(cancellationToken);

            return avatarUrl;
        }
    }


    public Task<Result> Remove(MemberContext memberContext, FacilityAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyManagementService.BuildFacilityKey(request.AccountId, request.FacilityId);

        return Validate()
            .Map(() => _awsImageManagementService.Delete(_options.BucketName, key, cancellationToken))
            .Bind(_ => UpdateMember());


        Result Validate()
        {
            var validator = new FacilityAvatarRequestValidator(_context, memberContext);
            return validator.ValidateRemove(request, cancellationToken).ToResult();
        }


        async Task<Result> UpdateMember()
        {
            var facility = await _context.Facilities
                .SingleAsync(m => m.Id == request.FacilityId, cancellationToken);

            facility.AvatarUrl = null;
            _context.Facilities.Update(facility);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }


    private readonly IAwsImageManagementService _awsImageManagementService;
    private readonly AetherDbContext _context;
    private readonly AvatarManagementServiceOptions _options;
}