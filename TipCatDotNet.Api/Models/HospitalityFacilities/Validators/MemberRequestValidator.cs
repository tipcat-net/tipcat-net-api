using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using TipCatDotNet.Api.Data;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class MemberRequestValidator : AbstractValidator<MemberRequest>
    {
        public MemberRequestValidator(MemberContext memberContext, AetherDbContext context)
        {
            _memberContext = memberContext;
            _context = context;

            RuleFor(x => x.AccountId)
                .NotNull()
                .GreaterThan(0)
                .Equal(memberContext.AccountId)
                .WithMessage("The current member does not belong to the target account.");
        }


        public ValidationResult ValidateAdd(MemberRequest request)
        {
            RuleFor(x => x.AccountId)
                .MustAsync((request, accountId, cancellationToken) => IsAccountHasNoManager(request.Permissions, accountId, cancellationToken))
                .WithMessage("The target account has a manager already.");

            RuleFor(x => x.Email)
                .NotEmpty();
            return this.Validate(request);
        }


        public ValidationResult ValidateRemove(MemberRequest request)
        {
            RuleFor(x => x.Id)
                .NotNull()
                .NotEqual(_memberContext.Id)
                .WithMessage("You can't remove yourself.");

            RuleFor(x => x.AccountId)
                .MustAsync((request, accountId, cancellationToken) => TargetMemberBelongToAccount(request.Id, accountId, cancellationToken))
                .WithMessage("The target member does not belong to the target account.");
            return this.Validate(request);
        }


        public ValidationResult ValidateOther(MemberRequest request)
        {
            RuleFor(x => x.AccountId)
                .MustAsync((request, accountId, cancellationToken) => TargetMemberBelongToAccount(request.Id, accountId, cancellationToken))
                .WithMessage("The target member does not belong to the target account.");
            return this.Validate(request);
        }


        private async Task<bool> IsAccountHasNoManager(MemberPermissions permissions, int? accountId, CancellationToken cancellationToken = default)
        {
            if (permissions != MemberPermissions.Manager)
                return true;

            return !await _context.Members
                .AnyAsync(m => m.AccountId == accountId && m.Permissions == MemberPermissions.Manager, cancellationToken);
        }


        private async Task<bool> TargetMemberBelongToAccount(int? memberId, int? accountId, CancellationToken cancellationToken)
        {
            var isRequestedMemberBelongsToAccount = await _context.Members
                .Where(m => m.Id == memberId && m.AccountId == accountId)
                .AnyAsync(cancellationToken);

            if (isRequestedMemberBelongsToAccount)
                return true;

            return false;
        }


        private readonly MemberContext _memberContext;
        private readonly AetherDbContext _context;
    }
}
