using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using TipCatDotNet.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator(AetherDbContext context)
        {
            _context = context;

            RuleFor(x => x.MemberId)
                .NotEmpty()
                .MustAsync((memberId, cancellationToken) => MemberIsExist(memberId, cancellationToken));
            RuleFor(x => x.TipsAmount).NotEmpty();
            RuleFor(x => x.TipsAmount.Amount)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(x => x.TipsAmount.Currency)
                .NotEmpty();
        }


        private async Task<bool> MemberIsExist(int memberId, CancellationToken cancellationToken)
        {
            var hasMember = await _context.Members
                .Where(m => m.Id == memberId)
                .AnyAsync(cancellationToken);

            if (hasMember)
                return true;

            return false;
        }


        private readonly AetherDbContext _context;
    }
}