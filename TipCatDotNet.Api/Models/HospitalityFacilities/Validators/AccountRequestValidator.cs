using FluentValidation;
using FluentValidation.Results;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Validators
{
    public class AccountRequestValidator : AbstractValidator<AccountRequest>
    {
        public AccountRequestValidator(MemberContext memberContext)
        {
            _memberContext = memberContext;
        }


        public ValidationResult ValidateAdd(in AccountRequest request)
        {
            RuleFor(x => x.Id)
                .Must(_ => _memberContext.AccountId is null);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("An account name should be specified.");
            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("An account address should be specified.");
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("A contact phone number should be specified.");

            return Validate(request);
        }


        public ValidationResult ValidateGet(in AccountRequest request)
        {
            RuleFor(x => x.Id)
                .NotNull()
                .GreaterThan(0)
                .Must(_ => _memberContext.AccountId is not null)
                .WithMessage("The member has no accounts.")
                .Equal(_memberContext.AccountId)
                .WithMessage("The member has no access to this account.");

            return Validate(request);
        }


        public ValidationResult ValidateUpdate(in AccountRequest request)
        {
            RuleFor(x => x.Id)
                .NotNull()
                .WithMessage("Provided account details have no ID.")
                .GreaterThan(0)
                .Equal(_memberContext.AccountId)
                .WithMessage("An account doesn't belongs to the current member.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("An account name should be specified.");
            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("An account address should be specified.");
            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("A contact phone number should be specified.");

            return Validate(request);
        }


        private readonly MemberContext _memberContext;
    }
}