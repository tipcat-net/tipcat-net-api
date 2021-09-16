using FluentValidation;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class MemberRequestAddValidator : MemberRequestValidator
    {
        public MemberRequestAddValidator()
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0);

            RuleFor(x => x.Email).NotEmpty();
        }
    }
}