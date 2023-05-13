using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;

/// <summary>
/// Specifies that the value must be a valid platform type string.
/// </summary>
/// <example>C1, G8, or M12</example>
public class TypeSchemaAttribute : RegularExpressionAttribute
{
    /// <inheritdoc cref="TypeSchemaAttribute"/>
    public TypeSchemaAttribute() : base(@"^[CGM][0-9]{1,2}$")
    {
    }
}
