using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using TipCatDotNet.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class FacilityRequestValidator : AbstractValidator<FacilityRequest>
    {
        public FacilityRequestValidator(MemberContext? memberContext, AetherDbContext context)
        {
            _context = context;
            _memberContext = memberContext!;
        }


        public ValidationResult ValidateAdd(FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .Equal(_memberContext!.AccountId)
                .WithMessage("The current member does not belong to the target account.");

            RuleFor(x => x.Name)
                .NotEmpty();
            return this.Validate(request);
        }


        public ValidationResult ValidateAddDefault(FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .MustAsync((accountId, cancellationToken) => TargetAccountHasNoDefault(accountId, cancellationToken))
                .WithMessage("The target account already has default facility.");
            return this.Validate(request);
        }


        public ValidationResult ValidateTransferMember(FacilityRequest request)
        {
            RuleFor(x => x.Id)
                .NotNull()
                .GreaterThan(0)
                .MustAsync((id, cancellationToken) => TargetMemberFacilityIsEqualToActualOne(_memberContext!.Id, id, cancellationToken))
                .WithMessage("The target account already has default facility.");
            return this.Validate(request);
        }


        public ValidationResult ValidateGetAll(FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .Equal(_memberContext!.AccountId)
                .WithMessage("The current member does not belong to the target account.");
            return this.Validate(request);
        }


        public ValidationResult ValidateGetOrUpdate(FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .Equal(_memberContext!.AccountId)
                .WithMessage("The current member does not belong to the target account.");

            RuleFor(x => x.Id)
                .NotNull()
                .GreaterThan(0)
                .MustAsync((id, cancellationToken) => TargetFacilityBelongsToAccount(id, _memberContext.AccountId, cancellationToken))
                .WithMessage("The target member does not belong to the target account.");
            return this.Validate(request);
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


        private async Task<bool> TargetMemberFacilityIsEqualToActualOne(int? memberId, int? targetFacilityId, CancellationToken cancellationToken)
        {
            var isEquivalentFacilities = await _context.Members
                .Where(m => m.Id == memberId && m.FacilityId == targetFacilityId)
                .AnyAsync(cancellationToken);

            if (isEquivalentFacilities)
                return false;

            return true;
        }


        private async Task<bool> TargetFacilityBelongsToAccount(int? facilityId, int? accountId, CancellationToken cancellationToken)
        {
            var isTargetFacilityBelongsToAccount = await _context.Facilities
                .Where(f => f.Id == facilityId && f.AccountId == accountId)
                .AnyAsync(cancellationToken);

            if (isTargetFacilityBelongsToAccount)
                return true;

            return false;
        }


        private async Task<bool> TargetAccountHasNoDefault(int? targetAccountId, CancellationToken cancellationToken)
        {
            var isDefaultFacilityExist = await _context.Facilities
                .Where(f => f.AccountId == targetAccountId && f.IsDefault == true)
                .AnyAsync(cancellationToken);

            if (isDefaultFacilityExist)
                return false;

            return true;
        }


        private readonly MemberContext _memberContext;
        private readonly AetherDbContext _context;
    }
}