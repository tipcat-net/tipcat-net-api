using System;
using System.Linq;
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
        IAwsAvatarManagementService awsImageManagementService)
    {
        _awsImageManagementService = awsImageManagementService;
        _context = context;
        _options = options.CurrentValue;
    }


    public Task<Result<string>> AddOrUpdate(MemberContext memberContext, FacilityAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyHelper.BuildFacilityKey(request.AccountId, request.FacilityId);

        return Validate()
            .Bind(() => _awsImageManagementService.Upload(_options.BucketName, request.File!, key, cancellationToken))
            .Bind(avatarUrl => UpdateMember(avatarUrl, request.FacilityId, cancellationToken));


        Result Validate()
        {
            var validator = new FacilityAvatarRequestValidator(_context, memberContext);
            return validator.ValidateAdd(request, cancellationToken).ToResult();
        }
    }


    public Task<Result> Remove(MemberContext memberContext, FacilityAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyHelper.BuildFacilityKey(request.AccountId, request.FacilityId);

        return Validate()
            .Map(() => _awsImageManagementService.Delete(_options.BucketName, key, cancellationToken))
            .Bind(_ => UpdateMember(null, request.FacilityId, cancellationToken))
            .Bind(_ => Result.Success());


        Result Validate()
        {
            var validator = new FacilityAvatarRequestValidator(_context, memberContext);
            return validator.ValidateRemove(request, cancellationToken).ToResult();
        }
    }


    public Task<Result<string>> UseParent(MemberContext memberContext, FacilityAvatarRequest request, CancellationToken cancellationToken = default)
    {
        return Validate()
            .Bind(GetParentAvatarUrl)
            .Bind(avatarUrl => UpdateMember(avatarUrl, request.FacilityId, cancellationToken));


        Result Validate()
        {
            var validator = new FacilityAvatarRequestValidator(_context, memberContext);
            return validator.ValidateUseParent(request, cancellationToken).ToResult();
        }


        async Task<Result<string>> GetParentAvatarUrl()
        {
            var avatarUrl = await _context.Accounts
                .Where(a => a.Id == request.AccountId)
                .Select(a => a.AvatarUrl)
                .SingleOrDefaultAsync(cancellationToken);

            return avatarUrl is null 
                ? Result.Failure<string>("The parent account has no avatar.") 
                : avatarUrl!;
        }
    }


    private async Task<Result<string>> UpdateMember(string? avatarUrl, int facilityId, CancellationToken cancellationToken)
    {
        var facility = await _context.Facilities
            .SingleAsync(m => m.Id == facilityId, cancellationToken);

        facility.AvatarUrl = avatarUrl;
        facility.Modified = DateTime.UtcNow;
        _context.Facilities.Update(facility);

        await _context.SaveChangesAsync(cancellationToken);

        return avatarUrl ?? string.Empty;
    }


    private readonly IAwsAvatarManagementService _awsImageManagementService;
    private readonly AetherDbContext _context;
    private readonly AvatarManagementServiceOptions _options;
}