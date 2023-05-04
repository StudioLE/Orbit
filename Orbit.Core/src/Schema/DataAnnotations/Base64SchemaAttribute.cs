using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;

public class Base64SchemaAttribute : RegularExpressionAttribute
{
    public const string Base64Regex = "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";

    /// <inheritdoc />
    public Base64SchemaAttribute() : base(Base64Regex)
    {
    }
}
