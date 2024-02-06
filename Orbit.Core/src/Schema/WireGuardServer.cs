using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the WireGuard interface of an instance.
/// </summary>
public class WireGuardServer
{
    /// <summary>
    /// The name of the WireGuard interface.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The WireGuard listen port.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; }

    /// <summary>
    /// The private key of the WireGuard interface.
    /// </summary>
    [Base64Schema]
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// The public key of the WireGuard interface.
    /// </summary>
    [Base64Schema]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// The address of the DNS resolver for the network.
    /// </summary>
    public string[] Dns { get; set; } = Array.Empty<string>();
}
