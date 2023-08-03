using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for the network configuration of an <see cref="Instance"/>.
/// </summary>
public sealed class Network : IEntity, IHasValidationAttributes
{
    /// <summary>
    /// The name of the network.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The number of the network.
    /// </summary>
    [Range(1, 64)]
    public int Number { get; set; }

    /// <summary>
    /// The name of the server hosting the network.
    /// </summary>
    [NameSchema]
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// The name of the server network interface.
    /// </summary>
    [NameSchema]
    public string Interface { get; set; } = string.Empty;

    /// <summary>
    /// The external IPv4 of the network.
    /// </summary>
    [IPSchema]
    public string ExternalIPv4 { get; set; } = string.Empty;

    /// <summary>
    /// The external IPv6 of the network.
    /// </summary>
    public string ExternalIPv6 { get; set; } = string.Empty;

    [ValidateComplexType]
    public WireGuardServer WireGuard { get; set; } = new();
}
