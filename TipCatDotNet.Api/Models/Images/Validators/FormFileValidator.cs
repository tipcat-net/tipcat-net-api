using System.Collections.Generic;
using System.IO;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.Images.Validators;

public class FormFileValidator : AbstractValidator<FormFile?>
{
    public new ValidationResult Validate(FormFile? file)
    {
        if (file is null)
            return new ValidationResult(new List<ValidationFailure>(1)
            {
                new(nameof(file), "Can't read the file. Probably, a file size exceeds 5MB, or a Content-Description header isn't set to 'multipart/form-data'.")
            });

        RuleFor(x => x)
            .Must(IsImageFormatSupports)
            .WithMessage("Images of a format like this aren't supported. Supported formats are JPEG and PNG.");

        return base.Validate(file);
    }


    private static bool IsImageFormatSupports(FormFile? file)
    {
        var extension = Path.GetExtension(file!.FileName).ToLower();
        return extension is ".jpg" or ".jpeg" or ".png";
    }
}