using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;

public class NameSchemaAttribute : RegularExpressionAttribute
{
    /// <inheritdoc />
    public NameSchemaAttribute() : base(@"^[a-z0-9][a-z0-9-]{0,14}[a-z0-9]$")
    {
    }
}
