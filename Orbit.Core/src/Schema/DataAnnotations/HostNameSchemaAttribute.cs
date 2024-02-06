using System.ComponentModel.DataAnnotations;

namespace Orbit.Schema.DataAnnotations;

/// <summary>
/// Specifies that the value must be a valid hostname.
/// </summary>
public class HostNameSchemaAttribute : RegularExpressionAttribute
{
    // ReSharper disable once InconsistentNaming
    private const string IPv4Regex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
    private const string HostnameRegex = @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$";

    /// <inheritdoc cref="HostNameSchemaAttribute" />
    public HostNameSchemaAttribute() : base($"({IPv4Regex}|{HostnameRegex})")
    {
    }
}
