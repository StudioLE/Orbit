using System.ComponentModel.DataAnnotations;
using Orbit.Utils.Networking;

namespace Orbit.Schema.DataAnnotations;

/// <summary>
/// Specifies that the value must be a valid IPv4.
/// </summary>
// ReSharper disable once InconsistentNaming
public class IPv4SchemaAttribute : RegularExpressionAttribute
{
    /// <inheritdoc cref="IPv4SchemaAttribute"/>
    public IPv4SchemaAttribute() : base(IPHelpers.IPv4Regex)
    {
    }
}
