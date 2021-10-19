using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TipCatDotNet.Api.Data;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class FacilityRequestValidator : AbstractValidator<FacilityRequest>
    {
        public FacilityRequestValidator(MemberContext? memberContext, AetherDbContext context, FacilityValidateMethods methodType)
        {
            _context = context;

            When(m => methodType == FacilityValidateMethods.Add, () =>
            {
                RuleFor(x => x.AccountId)
                    .NotNull()
                    .GreaterThan(0)
                    .Equal(memberContext!.AccountId)
                    .WithMessage("The current member does not belong to the target account.");

                RuleFor(x => x.Name)
                    .NotEmpty();
            });


            When(m => methodType == FacilityValidateMethods.AddDefault, () =>
            {
                RuleFor(x => x.AccountId)
                    .NotNull()
                    .GreaterThan(0);
            });


            When(m => methodType == FacilityValidateMethods.TransferMember, () =>
            {
                RuleFor(x => x.Id)
                    .NotNull()
                    .GreaterThan(0)
                    .MustAsync((id, cancellationToken) => TargetMemberFacilityIsEqualToActualOne(memberContext!.Id, id, cancellationToken))
                    .WithMessage("The target account already has default facility.");
            });


            When(m => methodType == FacilityValidateMethods.Update || methodType == FacilityValidateMethods.Get || methodType == FacilityValidateMethods.GetAll, () =>
            {
                RuleFor(x => x.AccountId)
                    .NotNull()
                    .GreaterThan(0)
                    .Equal(memberContext!.AccountId)
                    .WithMessage("The current member does not belong to the target account.");

                When(m => methodType == FacilityValidateMethods.Update || methodType == FacilityValidateMethods.Get, () =>
                {
                    RuleFor(x => x.Id)
                        .NotNull()
                        .GreaterThan(0)
                        .MustAsync((id, cancellationToken) => TargetFacilityBelongsToAccount(id, memberContext.AccountId, cancellationToken))
                        .WithMessage("The target member does not belong to the target account.");
                });
            });
        }


        private async Task<bool> TargetAccountHasNoDefault(int targetAccountId, CancellationToken cancellationToken)
        {
            var isDefaultFacilityExist = await _context.Facilities
                .Where(f => f.AccountId == targetAccountId && f.IsDefault == true)
                .AnyAsync(cancellationToken);

            if (isDefaultFacilityExist)
                return false;

            return true;
        }


        public async Task<bool> TargetMemberFacilityIsEqualToActualOne(int? memberId, int? targetFacilityId, CancellationToken cancellationToken)
        {
            var isEquivalentFacilities = await _context.Members
                .Where(m => m.Id == memberId && m.FacilityId == targetFacilityId)
                .AnyAsync(cancellationToken);

            if (isEquivalentFacilities)
                return false;

            return true;
        }


        public async Task<bool> TargetFacilityBelongsToAccount(int? facilityId, int? accountId, CancellationToken cancellationToken)
        {
            var isTargetFacilityBelongsToAccount = await _context.Facilities
                .Where(f => f.Id == facilityId && f.AccountId == accountId)
                .AnyAsync(cancellationToken);

            if (isTargetFacilityBelongsToAccount)
                return true;

            return false;
        }


        private readonly AetherDbContext _context;
    }
}