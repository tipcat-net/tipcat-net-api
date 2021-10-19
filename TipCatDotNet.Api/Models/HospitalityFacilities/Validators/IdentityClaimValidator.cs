using FluentValidation;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class IdentityClaimValidator : AbstractValidator<string?>
    {
        public IdentityClaimValidator()
        {
            RuleFor(x => x)
                .NotEmpty();
        }
    }
}