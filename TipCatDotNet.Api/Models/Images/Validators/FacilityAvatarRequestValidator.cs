using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Images.Validators;

public class FacilityAvatarRequestValidator : AbstractValidator<FacilityAvatarRequest>
{
    public FacilityAvatarRequestValidator(AetherDbContext context, MemberContext memberContext)
    {
        _context = context;
        _memberContext = memberContext;
    }


    public ValidationResult ValidateAdd(FacilityAvatarRequest request, CancellationToken cancellationToken)
    {
        var fileValidator = new FormFileValidator();
        var fileValidationResult = fileValidator.Validate(request.File);
        if (fileValidationResult.IsValid)
            return Validate(request, cancellationToken);

        return fileValidationResult;
    }


    public ValidationResult ValidateRemove(FacilityAvatarRequest request, CancellationToken cancellationToken) 
        => Validate(request, cancellationToken);


    private ValidationResult Validate(FacilityAvatarRequest request, CancellationToken cancellationToken)
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .Equal(_memberContext.AccountId!.Value)
            .WithMessage("The current member doesn't belong to a target account");

        RuleFor(x => x.FacilityId)
            .NotEmpty();

        RuleFor(x => x)
            .MustAsync((avatarRequest, _) => IsTargetFacilityBelongsToAccount(avatarRequest, cancellationToken))
            .WithMessage("The target facility doesn't belong to a target account");

        return Validate(request);
    }


    private Task<bool> IsTargetFacilityBelongsToAccount(FacilityAvatarRequest request, CancellationToken cancellationToken)
        => _context.Facilities
            .AnyAsync(f => f.Id == request.FacilityId && f.AccountId == request.AccountId, cancellationToken);


    private readonly AetherDbContext _context;
    private readonly MemberContext _memberContext;
}