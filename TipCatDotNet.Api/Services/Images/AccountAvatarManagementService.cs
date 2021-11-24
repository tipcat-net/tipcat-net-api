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

public class AccountAvatarManagementService : IAvatarManagementService<AccountAvatarRequest>
{
    public AccountAvatarManagementService(IOptionsMonitor<AvatarManagementServiceOptions> options, AetherDbContext context,
        IAwsImageManagementService awsImageManagementService)
    {
        _awsImageManagementService = awsImageManagementService;
        _context = context;
        _options = options.CurrentValue;
    }


    public Task<Result<string>> AddOrUpdate(MemberContext memberContext, AccountAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyHelper.BuildAccountKey(request.AccountId);

        return Validate()
            .Bind(() => _awsImageManagementService.Upload(_options.BucketName, request.File!, key, cancellationToken))
            .Bind(UpdateMember);


        Result Validate()
        {
            var validator = new AccountAvatarRequestValidator(memberContext);
            return validator.ValidateAdd(request, cancellationToken).ToResult();
        }


        async Task<Result<string>> UpdateMember(string avatarUrl)
        {
            var account = await _context.Accounts
                .SingleAsync(m => m.Id == request.AccountId, cancellationToken);

            account.AvatarUrl = avatarUrl;
            _context.Accounts.Update(account);

            await _context.SaveChangesAsync(cancellationToken);

            return avatarUrl;
        }
    }


    public Task<Result> Remove(MemberContext memberContext, AccountAvatarRequest request, CancellationToken cancellationToken = default)
    {
        var key = AvatarKeyHelper.BuildAccountKey(request.AccountId);

        return Validate()
            .Map(() => _awsImageManagementService.Delete(_options.BucketName, key, cancellationToken))
            .Bind(_ => UpdateMember());


        Result Validate()
        {
            var validator = new AccountAvatarRequestValidator(memberContext);
            return validator.ValidateRemove(request, cancellationToken).ToResult();
        }


        async Task<Result> UpdateMember()
        {
            var account = await _context.Accounts
                .SingleAsync(m => m.Id == request.AccountId, cancellationToken);

            account.AvatarUrl = null;
            _context.Accounts.Update(account);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }


    private readonly IAwsImageManagementService _awsImageManagementService;
    private readonly AetherDbContext _context;
    private readonly AvatarManagementServiceOptions _options;
}