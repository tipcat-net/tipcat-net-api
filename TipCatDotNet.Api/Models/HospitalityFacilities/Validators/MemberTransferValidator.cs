using FluentValidation;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class MemberTransferValidator : AbstractValidator<(int facilityId, int memberId, int accountId)>
    {
        public MemberTransferValidator(MemberContext memberContext)
        {
            RuleFor(x => x.facilityId).NotEmpty();
            RuleFor(x => x.memberId).NotEmpty();
            RuleFor(x => x.accountId)
                .NotEmpty()
                .Equal(memberContext.AccountId ?? 0)
                .WithMessage("The current member does not belong to the target account.");
        }
    }
}