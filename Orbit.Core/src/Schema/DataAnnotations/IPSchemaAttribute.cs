using System.ComponentModel.DataAnnotations;

namespace Orbit.Core.Schema.DataAnnotations;

// ReSharper disable once InconsistentNaming
public class IPSchemaAttribute : RegularExpressionAttribute
{
    // ReSharper disable once InconsistentNaming
    private const string IPv4Regex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

    /// <inheritdoc />
    public IPSchemaAttribute() : base(IPv4Regex)
    {
    }
}
