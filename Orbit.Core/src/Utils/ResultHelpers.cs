using StudioLE.Extensions.System;
using StudioLE.Results;

namespace Orbit.Core.Utils;

/// <summary>
/// Methods to help with <see cref="IResult"/>.
/// </summary>
public static class ResultHelpers
{
    /// <summary>
    /// Validate the <paramref name="result"/>.
    /// Throw an exception if it is not a success.
    /// </summary>
    /// <returns>The value of the <see cref="Success{T}"/></returns>
    public static T GetValueOrThrow<T>(this IResult<T> result, string? contextMessage = null)
    {
        if (result is Success<T> success)
            return success;
        string message = contextMessage is null
            ? string.Empty
            : contextMessage + Environment.NewLine;
        if (result.Errors.Any())
            message += result.Errors.Join() + Environment.NewLine;
        throw new(message);
    }

    /// <summary>
    /// Validate the <paramref name="result"/>.
    /// Throw an exception if it is not a success.
    /// </summary>
    public static void ThrowOnFailure(this IResult result, string? contextMessage = null)
    {
        if (result is Success)
            return;
        string message = contextMessage is null
            ? string.Empty
            : contextMessage + Environment.NewLine;
        if (result.Errors.Any())
            message += result.Errors.Join() + Environment.NewLine;
        throw new(message);
    }
}
