using FluentValidation;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class MemberRequestValidator : AbstractValidator<MemberRequest>
    {
        public MemberRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Permissions)
                .NotEmpty()
                .NotEqual(MemberPermissions.None);
        }
    }
}
