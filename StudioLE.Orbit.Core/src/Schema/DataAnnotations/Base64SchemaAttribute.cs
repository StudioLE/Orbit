using System.ComponentModel.DataAnnotations;

namespace StudioLE.Orbit.Schema.DataAnnotations;

/// <summary>
/// Specifies that the value must be a valid base64 string.
/// </summary>
public class Base64SchemaAttribute : RegularExpressionAttribute
{
    /// <summary>
    /// A regex that matches a base64 string.
    /// </summary>
    public const string Base64Regex = "^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";

    /// <inheritdoc cref="Base64SchemaAttribute" />
    public Base64SchemaAttribute() : base(Base64Regex)
    {
    }
}
