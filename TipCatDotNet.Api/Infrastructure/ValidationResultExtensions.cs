using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FluentValidation.Results;

namespace TipCatDotNet.Api.Infrastructure
{
    public static class ValidationResultExtensions
    {
        public static Result ToFailureResult(this ValidationResult target)
            => Result.Failure(BuildString(target.Errors));


        public static Result<string> ToFailureStringResult(this ValidationResult target)
                    => Result.Failure<string>(BuildString(target.Errors));


        private static string BuildString(List<ValidationFailure> errors)
            => string.Join(' ', errors);
    }
}
