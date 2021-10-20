using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.Api.Models.Payments.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator(AetherDbContext context)
        {
            _context = context;

            RuleFor(x => x.MemberId)
                .NotEmpty()
                .MustAsync(MemberIsExist);
            RuleFor(x => x.TipsAmount).NotEmpty();
            RuleFor(x => x.TipsAmount.Amount)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(x => x.TipsAmount.Currency)
                .NotEmpty();
        }


        private async Task<bool> MemberIsExist(int memberId, CancellationToken cancellationToken)
        {
            return await _context.Members
                .Where(m => m.Id == memberId)
                .AnyAsync(cancellationToken);
        }


        private readonly AetherDbContext _context;
    }
}