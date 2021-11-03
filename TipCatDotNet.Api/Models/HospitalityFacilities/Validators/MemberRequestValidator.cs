using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using TipCatDotNet.Api.Data;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Models.Permissions.Enums;

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


        public ValidationResult ValidateAdd(in MemberRequest request)
        {
            RuleFor(x => x.AccountId)
                .MustAsync((req, accountId, cancellationToken) => IsAccountHasNoManager(req.Permissions, accountId, cancellationToken))
                .WithMessage("The target account has a manager already.");

            RuleFor(x => x.Email)
                .NotEmpty();

            RuleFor(x => x.Email)
                .MustAsync(HasMemberWithSpecifiedEmail)
                .WithMessage("A member with an email address '{PropertyValue}' already has an account in the system.");

            return Validate(request);


            async Task<bool> HasMemberWithSpecifiedEmail(string? email, CancellationToken cancellationToken)
            {
                if (email is null)
                    return false;

                return !await _context.Members.AnyAsync(m => m.Email == email, cancellationToken);
            }
        }


        public ValidationResult ValidateInvite(in MemberRequest request) 
            => ValidateGeneral(request);


        public ValidationResult ValidateRemove(in MemberRequest request)
        {
            RuleFor(x => x.Id)
                .NotNull()
                .NotEqual(_memberContext.Id)
                .WithMessage("You can't remove yourself.");

            return ValidateGeneral(request);
        }


        public ValidationResult ValidateGeneral(in MemberRequest request)
        {
            RuleFor(x => x.AccountId)
                .MustAsync((req, accountId, cancellationToken) => TargetMemberBelongToAccount(req.Id, accountId, cancellationToken))
                .WithMessage("The target member does not belong to the target account.");
            
            return Validate(request);
        }


        private async Task<bool> IsAccountHasNoManager(MemberPermissions permissions, int? accountId, CancellationToken cancellationToken)
        {
            if (permissions != MemberPermissions.Manager)
                return true;

            return !await _context.Members
                .AnyAsync(m => m.AccountId == accountId && m.Permissions == MemberPermissions.Manager, cancellationToken);
        }


        private Task<bool> TargetMemberBelongToAccount(int? memberId, int? accountId, CancellationToken cancellationToken)
        {
            var query = _context.Members.AsQueryable();
            if (memberId is not null)
                query = query.Where(m => m.Id == memberId);
            
            if (accountId is not null)
                query = query.Where(m => m.AccountId == accountId);

            return query.AnyAsync(cancellationToken);
        }


        private readonly MemberContext _memberContext;
        private readonly AetherDbContext _context;
    }
}
