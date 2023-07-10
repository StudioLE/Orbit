using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for the network configuration of an <see cref="Instance"/>.
/// </summary>
public sealed class Network : IEntity
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
    /// The WireGuard private key.
    /// </summary>
    [Base64Schema]
    public string WireGuardPrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// The WireGuard public key.
    /// </summary>
    [Base64Schema]
    public string WireGuardPublicKey { get; set; } = string.Empty;

    /// <summary>
    /// The WireGuard listen port.
    /// </summary>
    [Range(1, 65535)]
    public int WireGuardPort { get; set; }

    public string GetInternalIPv4(Instance instance)
    {
        return $"10.{Number}.{instance.Number}.0";
    }

    public string GetInternalIPv6(Instance instance)
    {
        return $"fc00:{Number}:{instance.Number}::";
    }
}
