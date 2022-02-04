using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Company.Validators;

public class SupportRequestValidator : AbstractValidator<SupportRequest>
{
    public SupportRequestValidator(MemberContext? memberContext)
    {
        _memberContext = memberContext;
    }


    public ValidationResult Validate(in SupportRequest request)
    {
        if (string.IsNullOrWhiteSpace(_memberContext!.Email))
            return new ValidationResult(new List<ValidationFailure>(1)
                { new(nameof(_memberContext.Email), "The target member has no email.") });

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("The request content wasn't specified.");

        return base.Validate(request);
    }


    private readonly MemberContext? _memberContext;
}