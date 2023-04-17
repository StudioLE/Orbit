using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using StudioLE.Core.System;

namespace Orbit.Core.Utils.DataAnnotations;

public static class ValidationHelpers
{
    private const bool ValidateAllProperties = true;

    public static IReadOnlyCollection<string> Validate(this object @this, bool validateAllProperties = ValidateAllProperties)
    {
        List<ValidationResult> results = new();
        ValidationContext context = new(@this);
        return Validator.TryValidateObject(@this, context, results, validateAllProperties)
            ? Array.Empty<string>()
            : results
                .Select(x => x.ErrorMessage)
                .OfType<string>()
                .ToArray();
    }

    public static bool TryValidate(this IHasValidationAttributes @this, out IReadOnlyCollection<string> errors, bool validateAllProperties = ValidateAllProperties)
    {
        List<ValidationResult> results = new();
        ValidationContext context = new(@this);
        if (Validator.TryValidateObject(@this, context, results, validateAllProperties))
        {
            errors = Array.Empty<string>();
            return true;
        }
        errors = results
            .Select(x => x.ErrorMessage)
            .OfType<string>()
            .ToArray();
        return false;
    }

    public static bool TryValidate(this IHasValidationAttributes @this, ILogger logger, LogLevel logLevel = LogLevel.Error, bool validateAllProperties = ValidateAllProperties)
    {
        if (TryValidate(@this, out IReadOnlyCollection<string> errors, validateAllProperties))
            return true;
        logger.Log(logLevel, errors.Join());
        return false;
    }
}
