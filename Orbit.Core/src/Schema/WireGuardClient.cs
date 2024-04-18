using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the WireGuard interface of an instance.
/// </summary>
public record struct WireGuardClient()
{
    /// <summary>
    /// The name of the WireGuard interface.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The WireGuard server.
    /// </summary>
    [NameSchema]
    public ServerId Server { get; set; }

    /// <summary>
    /// The addresses of the WireGuard interface.
    /// </summary>
    public string[] Addresses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The WireGuard listen port.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; set; } = 0;

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
    /// The destination IP ranges to be routed through the WireGuard server.
    /// </summary>
    [IPv4Schema]
    // ReSharper disable once InconsistentNaming
    public string[] AllowedIPs { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The endpoint of the WireGuard server including the port.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
}
