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
    /// The external IPv4 of the network.
    /// </summary>
    [IPSchema]
    public string ExternalIPv4 { get; set; } = string.Empty;

    /// <summary>
    /// The external IPv6 of the network.
    /// </summary>
    public string ExternalIPv6 { get; set; } = string.Empty;

    /// <summary>
    /// The address of the DNS resolver for the network.
    /// </summary>
    [IPSchema]
    public string Dns { get; set; } = string.Empty;

    [ValidateComplexType]
    public WireGuardServer WireGuard { get; set; } = new();

    public string GetInternalIPv4(Instance instance)
    {
        return $"10.0.{Number}.{instance.Number}";
    }

    public string GetInternalIPv6(Instance instance)
    {
        return $"fc00::0:{Number}:{instance.Number}";
    }

    public string GetWireGuardIPv4(Instance instance)
    {
        return $"10.1.{Number}.{instance.Number}";
    }

    public string GetWireGuardIPv6(Instance instance)
    {
        return $"fc00::1:{Number}:{instance.Number}";
    }
}
