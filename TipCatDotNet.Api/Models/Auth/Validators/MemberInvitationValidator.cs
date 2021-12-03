using FluentValidation;
using FluentValidation.Results;
using TipCatDotNet.Api.Data.Models.Auth;
using TipCatDotNet.Api.Models.Auth.Enums;

namespace TipCatDotNet.Api.Models.Auth.Validators;

public class MemberInvitationValidator : AbstractValidator<MemberInvitation>
{
    public new ValidationResult Validate(MemberInvitation? invitation)
    {
        if (invitation is null)
            return new ValidationResult(new[] { new ValidationFailure(nameof(invitation), "No invitations found for that member") });

        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("No invitations found for that member.");

        RuleFor(x => x!.State)
            .NotEqual(InvitationStates.Accepted)
            .WithMessage("The invitation was accepted already.");

        return base.Validate(invitation);
    }
}