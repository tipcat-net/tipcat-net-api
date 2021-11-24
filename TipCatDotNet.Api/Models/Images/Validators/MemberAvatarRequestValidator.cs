using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
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


    public ValidationResult ValidateAdd(MemberAvatarRequest request, CancellationToken cancellationToken)
    {
        var fileValidator = new FormFileValidator();
        var fileValidationResult = fileValidator.Validate(request.File);
        if (fileValidationResult.IsValid)
            return Validate(request, cancellationToken);

        return fileValidationResult;
    }


    public ValidationResult ValidateRemove(MemberAvatarRequest request, CancellationToken cancellationToken) 
        => Validate(request, cancellationToken);


    private ValidationResult Validate(MemberAvatarRequest request, CancellationToken cancellationToken)
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

        return Validate(request);
    }


    private Task<bool> IsTargetMemberBelongsToAccount(MemberAvatarRequest request, CancellationToken cancellationToken)
        => _context.Members
            .AnyAsync(m => m.Id == request.MemberId && m.AccountId == request.AccountId, cancellationToken);


    private readonly AetherDbContext _context;
    private readonly MemberContext _memberContext;
}