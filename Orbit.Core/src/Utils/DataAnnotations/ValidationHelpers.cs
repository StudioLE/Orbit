using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using StudioLE.Core.System;

namespace Orbit.Core.Utils.DataAnnotations;

/// <summary>
/// Methods to help with <see cref="ValidationAttribute"/>.
/// </summary>
public static class ValidationHelpers
{
    private const bool ValidateAllProperties = true;

    /// <summary>
    /// Validate an object and return a collection of errors.
    /// </summary>
    /// <returns>A collection of errors or an empty collection if validation was successful.</returns>
    public static IReadOnlyCollection<string> Validate(
        this IHasValidationAttributes @this,
        bool validateAllProperties = ValidateAllProperties)
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

    /// <summary>
    /// Validate an object.
    /// </summary>
    /// <returns><see langword="true"/> if validation is successful.</returns>
    public static bool TryValidate(
        this IHasValidationAttributes @this,
        out IReadOnlyCollection<string> errors,
        bool validateAllProperties = ValidateAllProperties)
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


    /// <summary>
    /// Validate an object and log any validation errors to <paramref name="logger"/> using <paramref name="logLevel"/>.
    /// </summary>
    /// <returns><see langword="true"/> if validation is successful.</returns>
    public static bool TryValidate(
        this IHasValidationAttributes @this,
        ILogger logger,
        LogLevel logLevel = LogLevel.Error,
        bool validateAllProperties = ValidateAllProperties)
    {
        if (TryValidate(@this, out IReadOnlyCollection<string> errors, validateAllProperties))
            return true;
        logger.Log(logLevel, errors.Join());
        return false;
    }
}
