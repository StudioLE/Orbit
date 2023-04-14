using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;

public class IdSchemaAttribute : RegularExpressionAttribute
{
    /// <inheritdoc />
    public IdSchemaAttribute() : base(@"^(0[1-9]|[1-5][0-9]|6[0-4])-(0[1-9]|[1-5][0-9]|6[0-4])$")
    {
    }
}
