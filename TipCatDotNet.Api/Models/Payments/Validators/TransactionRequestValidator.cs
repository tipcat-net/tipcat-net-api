using FluentValidation;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.Api.Models.Payments.Validators
{
    public class TransactionRequestValidator : AbstractValidator<(int skip, int top)>
    {
        public TransactionRequestValidator()
        {
            RuleFor(x => x.skip)
                .NotNull()
                .GreaterThanOrEqualTo(0)
                .WithMessage("The number of skipped transactions has to be greater than or equal to zero!");

            RuleFor(x => x.top)
                .NotNull()
                .GreaterThanOrEqualTo(1)
                .WithMessage("The number of received transactions has to be greater than or equal to one!")
                .LessThanOrEqualTo(100)
                .WithMessage("The number of received transactions has to be less than or equal to hundred!");
        }
    }
}