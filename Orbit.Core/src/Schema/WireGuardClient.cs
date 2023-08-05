using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for the WireGuard interface of an instance.
/// </summary>
public sealed class WireGuardClient
{
    /// <summary>
    /// The name of the WireGuard interface.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The network of the WireGuard peer.
    /// </summary>
    [NameSchema]
    public string Network { get; set; } = string.Empty;

    /// <summary>
    /// Is the WireGuard peer external?
    /// </summary>
    public bool IsExternal { get; set; } = false;

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
    /// The pre-shared key of the WireGuard interface.
    /// </summary>
    [Base64Schema]
    public string PreSharedKey { get; set; } = string.Empty;

    /// <summary>
    /// The addresses of the WireGuard interface.
    /// </summary>
    [IPSchema]
    public string[] Addresses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The destination IP ranges to be routed through the WireGuard server.
    /// </summary>
    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string[] AllowedIPs { get; set; } = Array.Empty<string>();
}
