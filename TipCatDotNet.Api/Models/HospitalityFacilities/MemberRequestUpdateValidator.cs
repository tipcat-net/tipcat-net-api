using FluentValidation;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class MemberRequestUpdateValidator : MemberRequestValidator
    {
        public MemberRequestUpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0);
        }
    }
}