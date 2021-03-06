using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data;

namespace TipCatDotNet.Api.Models.Payments.Validators;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator(AetherDbContext context, CancellationToken cancellationToken)
    {
        _context = context;

        RuleFor(x => x)
            .NotEmpty() // Not working return 500
            .WithMessage("Something wrong with request data!");

        RuleFor(x => x.MemberId)
            .NotEmpty()
            .MustAsync((memberId, _) => IsMemberExist(memberId, cancellationToken))
            .WithMessage("The member with ID {PropertyValue} was not found.");

        RuleFor(x => x.TipsAmount)
            .NotEmpty();

        RuleFor(x => x.TipsAmount.Amount)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.TipsAmount.Currency)
            .NotEmpty() // Not working return 500
            .IsInEnum() // Not working return 500
            .WithMessage("The entered currency is not supported!");
    }


    public new ValidationResult Validate(PaymentRequest? request)
    {
        if (request is null)
            return new ValidationResult(new List<ValidationFailure>(1)
            {
                new(nameof(request),"Something wrong with request's data! Please check it out!")
            });

        return base.Validate(request);
    }


    private async Task<bool> IsMemberExist(int memberId, CancellationToken cancellationToken)
        => await _context.Members
            .Where(m => m.Id == memberId)
            .AnyAsync(cancellationToken);


    private readonly AetherDbContext _context;
}