using FluentValidation;
using FluentValidation.Results;
using TipCatDotNet.Api.Data.Models.Auth;

namespace TipCatDotNet.Api.Models.Auth.Validators
{
    public class MemberInvitationValidator : AbstractValidator<MemberInvitation>
    {
        public new ValidationResult Validate(MemberInvitation invitation)
        {
            RuleFor(x => x)
                .NotNull()
                .WithMessage("No invitations found for that member.");

            RuleFor(x => x)
                .NotEmpty()
                .WithMessage("The invitation was accepted already.");

            return base.Validate(invitation);
        }
    }
}
