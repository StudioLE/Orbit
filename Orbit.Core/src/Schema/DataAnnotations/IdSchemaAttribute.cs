using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;

/// <summary>
/// Specifies that the value must be a valid id string.
/// A number 0-64, a hyphen, and a number 0-64
/// </summary>
/// <example>01-01, 64-64</example>
public class IdSchemaAttribute : RegularExpressionAttribute
{
    /// <inheritdoc cref="IdSchemaAttribute" />
    public IdSchemaAttribute() : base("^(0[1-9]|[1-5][0-9]|6[0-4])-(0[1-9]|[1-5][0-9]|6[0-4])$")
    {
    }
}
