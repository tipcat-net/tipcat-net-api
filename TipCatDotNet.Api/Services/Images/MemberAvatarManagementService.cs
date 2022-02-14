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

public class MemberAvatarManagementService : IAvatarManagementService<MemberAvatarRequest>
{
    public MemberAvatarManagementService(IOptionsMonitor<AvatarManagementServiceOptions> options, AetherDbContext context,
        IAwsAvatarManagementService awsImageManagementService)
    {
        _awsImageManagementService = awsImageManagementService;
        _context = context;
        _options = options.CurrentValue;
    }


    public Task<Result<string>> AddOrUpdate(MemberContext memberContext, MemberAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyHelper.BuildMemberKey(request.AccountId, request.MemberId);

        return Validate()
            .Bind(() => _awsImageManagementService.Upload(_options.BucketName, request.File!, key, cancellationToken))
            .Bind(UpdateMember);


        Result Validate()
        {
            var validator = new MemberAvatarRequestValidator(_context, memberContext);
            return validator.ValidateAdd(request, cancellationToken).ToResult();
        }


        async Task<Result<string>> UpdateMember(string avatarUrl)
        {
            var member = await _context.Members
                .SingleAsync(m => m.Id == request.MemberId, cancellationToken);

            member.AvatarUrl = avatarUrl;
            _context.Members.Update(member);

            await _context.SaveChangesAsync(cancellationToken);

            return avatarUrl;
        }
    }


    public Task<Result> Remove(MemberContext memberContext, MemberAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyHelper.BuildMemberKey(request.AccountId, request.MemberId);

        return Validate()
            .Map(() => _awsImageManagementService.Delete(_options.BucketName, key, cancellationToken))
            .Bind(_ => UpdateMember());


        Result Validate()
        {
            var validator = new MemberAvatarRequestValidator(_context, memberContext);
            return validator.ValidateRemove(request, cancellationToken).ToResult();
        }


        async Task<Result> UpdateMember()
        {
            var member = await _context.Members
                .SingleAsync(m => m.Id == request.MemberId, cancellationToken);

            member.AvatarUrl = null;
            _context.Members.Update(member);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }


    public Task<Result<string>> UseParent(MemberContext memberContext, MemberAvatarRequest request, CancellationToken cancellationToken = default)
        => throw new System.NotImplementedException();


    private readonly IAwsAvatarManagementService _awsImageManagementService;
    private readonly AetherDbContext _context;
    private readonly AvatarManagementServiceOptions _options;
}