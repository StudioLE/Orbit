using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;


/// <summary>
/// Specifies that the value must be a valid name string.
/// Alpha-numeric with hyphens, 16 characters max, and must start and end with a letter or number.
/// </summary>
public class NameSchemaAttribute : RegularExpressionAttribute
{
    /// <inheritdoc cref="NameSchemaAttribute" />
    public NameSchemaAttribute() : base(@"^[a-z0-9][a-z0-9-]{0,14}[a-z0-9]$")
    {
    }
}
