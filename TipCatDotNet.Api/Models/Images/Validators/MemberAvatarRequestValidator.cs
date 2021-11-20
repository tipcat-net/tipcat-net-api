using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Images.Validators;

public class MemberAvatarRequestValidator : AbstractValidator<MemberAvatarRequest>
{
    public MemberAvatarRequestValidator(AetherDbContext context, MemberContext memberContext)
    {
        _context = context;
        _memberContext = memberContext;
    }


    public ValidationResult Validate(MemberAvatarRequest request, CancellationToken cancellationToken)
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .Equal(_memberContext.AccountId!.Value)
            .WithMessage("The current member doesn't belong to a target account");

        RuleFor(x => x.MemberId)
            .NotEmpty();

        RuleFor(x => x)
            .MustAsync((avatarRequest, _) => IsTargetMemberBelongsToAccount(avatarRequest, cancellationToken))
            .WithMessage("The target member doesn't belong to a target account");

        RuleFor(x => x.File)
            .NotNull()
            .Must(file => IsImageFormatSupports(file!))
            .WithMessage("Images of a format like this aren't supported. Supported formats are JPEG and PNG");
        
        return Validate(request);
    }


    private static bool IsImageFormatSupports(FormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLower();
        return extension is ".jpg" or ".jpeg" or ".png";
    }


    private Task<bool> IsTargetMemberBelongsToAccount(MemberAvatarRequest request, CancellationToken cancellationToken)
        => _context.Members
            .AnyAsync(m => m.Id == request.MemberId && m.AccountId == request.AccountId, cancellationToken);


    private readonly AetherDbContext _context;
    private readonly MemberContext _memberContext;
}