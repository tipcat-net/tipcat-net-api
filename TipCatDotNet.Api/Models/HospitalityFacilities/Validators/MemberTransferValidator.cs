using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class MemberTransferValidator : AbstractValidator<(int facilityId, int memberId, int accountId)>
    {
        public MemberTransferValidator(MemberContext memberContext, AetherDbContext context)
        {
            _context = context;
            _memberContext = memberContext;
        }


        public new ValidationResult Validate((int facilityId, int memberId, int accountId) request)
        {
            RuleFor(x => x.facilityId).NotEmpty();
            RuleFor(x => x.memberId).NotEmpty();
            RuleFor(x => x.accountId)
                .NotEmpty()
                .Equal(_memberContext.AccountId ?? 0)
                .WithMessage("The current member does not belong to the target account.");

            RuleFor(x => x.memberId)
                .MustAsync((memberId, cancellationToken) => TargetMemberFacilityIsEqualToActualOne(memberId, request.facilityId, cancellationToken))
                .WithMessage("Current and target account facilities are the same.");

            return base.Validate(request);
        }


        private async Task<bool> TargetMemberFacilityIsEqualToActualOne(int memberId, int targetFacilityId, CancellationToken cancellationToken)
            => !await _context.Members
                .Where(m => m.Id == memberId && m.FacilityId == targetFacilityId)
                .AnyAsync(cancellationToken);


        private readonly AetherDbContext _context;
        private readonly MemberContext _memberContext;
    }
}