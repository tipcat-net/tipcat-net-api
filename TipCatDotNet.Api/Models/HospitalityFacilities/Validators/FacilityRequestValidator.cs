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


        public FacilityRequestValidator(AetherDbContext context)
        {
            _context = context!;
            _memberContext = MemberContext.CreateEmpty();
        }


        public ValidationResult ValidateAdd(in FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .Equal(_memberContext.AccountId)
                .WithMessage("The current member does not belong to the target account.");

            RuleFor(x => x.Name)
                .NotEmpty();
            
            return Validate(request);
        }


        public ValidationResult ValidateAddDefault(in FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .MustAsync(TargetAccountHasNoDefault)
                .WithMessage("The target account already has default facility.");
            
            return Validate(request);
        }


        public ValidationResult ValidateTransferMember(in FacilityRequest request)
        {
            RuleFor(x => x.Id)
                .NotNull()
                .GreaterThan(0)
                .MustAsync((id, cancellationToken) => TargetMemberFacilityIsEqualToActualOne(_memberContext.Id, id, cancellationToken))
                .WithMessage("The target account already has default facility.");
            
            return Validate(request);
        }


        public ValidationResult ValidateGetAll(in FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .Equal(_memberContext.AccountId)
                .WithMessage("The current member does not belong to the target account.");
            
            return Validate(request);
        }


        public ValidationResult ValidateGetOrUpdate(in FacilityRequest request)
        {
            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .Equal(_memberContext.AccountId)
                .WithMessage("The current member does not belong to the target account.");

            RuleFor(x => x.Id)
                .NotNull()
                .GreaterThan(0)
                .MustAsync((id, cancellationToken) => TargetFacilityBelongsToAccount(id, _memberContext.AccountId, cancellationToken))
                .WithMessage("The target member does not belong to the target account.");
            
            return Validate(request);
        }


        private async Task<bool> TargetMemberFacilityIsEqualToActualOne(int? memberId, int? targetFacilityId, CancellationToken cancellationToken)
            => !await _context.Members
                .Where(m => m.Id == memberId && m.FacilityId == targetFacilityId)
                .AnyAsync(cancellationToken);


        private async Task<bool> TargetFacilityBelongsToAccount(int? facilityId, int? accountId, CancellationToken cancellationToken)
            => await _context.Facilities
                .Where(f => f.Id == facilityId && f.AccountId == accountId)
                .AnyAsync(cancellationToken);


        private async Task<bool> TargetAccountHasNoDefault(int? targetAccountId, CancellationToken cancellationToken)
            => !await _context.Facilities
                .Where(f => f.AccountId == targetAccountId && f.IsDefault == true)
                .AnyAsync(cancellationToken);


        private readonly MemberContext _memberContext;
        private readonly AetherDbContext _context;
    }
}