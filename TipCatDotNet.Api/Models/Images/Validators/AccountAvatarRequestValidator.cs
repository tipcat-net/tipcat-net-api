using System.Threading;
using FluentValidation;
using FluentValidation.Results;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Images.Validators;

public class AccountAvatarRequestValidator : AbstractValidator<AccountAvatarRequest>
{
    public AccountAvatarRequestValidator(MemberContext memberContext)
    {
        _memberContext = memberContext;
    }


    public ValidationResult ValidateAdd(AccountAvatarRequest request, CancellationToken cancellationToken)
    {
        var fileValidator = new FormFileValidator();
        var fileValidationResult = fileValidator.Validate(request.File);
        if (fileValidationResult.IsValid)
            return Validate(request, cancellationToken);

        return fileValidationResult;
    }


    public ValidationResult ValidateRemove(AccountAvatarRequest request, CancellationToken cancellationToken) 
        => Validate(request, cancellationToken);


    private ValidationResult Validate(AccountAvatarRequest request, CancellationToken cancellationToken)
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .Equal(_memberContext.AccountId!.Value)
            .WithMessage("The current member doesn't belong to a target account");

        return Validate(request);
    }


    private readonly MemberContext _memberContext;
}