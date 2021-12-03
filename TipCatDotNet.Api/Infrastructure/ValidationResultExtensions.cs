using System.Collections.Generic;
using CSharpFunctionalExtensions;
using FluentValidation.Results;

namespace TipCatDotNet.Api.Infrastructure;

public static class ValidationResultExtensions
{
    public static Result ToResult(this ValidationResult target)
        => target.IsValid
            ? Result.Success()
            : target.ToFailureResult();


    public static Result<T> ToResult<T>(this ValidationResult target, T successfulResult) 
        => target.IsValid 
            ? Result.Success(successfulResult) 
            : target.ToFailureResult<T>();


    public static Result ToFailureResult(this ValidationResult target)
        => Result.Failure(BuildString(target.Errors));


    public static Result<T> ToFailureResult<T>(this ValidationResult target)
        => Result.Failure<T>(BuildString(target.Errors));


    private static string BuildString(List<ValidationFailure> errors)
        => string.Join(' ', errors);
}