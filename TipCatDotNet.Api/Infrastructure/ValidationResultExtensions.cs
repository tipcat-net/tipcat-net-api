using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FluentValidation.Results;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class ValidationResultExtensions
    {
        public static Result ToResult(this ValidationResult target)
        {
            if (target.IsValid)
                return Result.Success();

            return target.ToFailureResult();
        }


        public static Result ToFailureResult(this ValidationResult target)
            => Result.Failure(BuildString(target.Errors));


        private static string BuildString(List<ValidationFailure> errors)
            => string.Join(' ', errors);
    }
}
