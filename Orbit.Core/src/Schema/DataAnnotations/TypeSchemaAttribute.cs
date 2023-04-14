using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;

public class TypeSchemaAttribute : RegularExpressionAttribute
{
    /// <inheritdoc />
    public TypeSchemaAttribute() : base(@"^[CGM][0-9]{1,2}$")
    {
    }
}
