using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the WireGuard interface of an instance.
/// </summary>
public readonly record struct WireGuardClient()
{
    /// <summary>
    /// The WireGuard interface.
    /// </summary>
    public Interface Interface { get; init; } = new();

    /// <summary>
    /// The WireGuard listen port.
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; init; } = 0;

    /// <summary>
    /// The private key of the WireGuard interface.
    /// </summary>
    [Base64Schema]
    public string PrivateKey { get; init; } = string.Empty;

    /// <summary>
    /// The public key of the WireGuard interface.
    /// </summary>
    [Base64Schema]
    public string PublicKey { get; init; } = string.Empty;

    /// <summary>
    /// The pre-shared key of the WireGuard interface.
    /// </summary>
    [Base64Schema]
    public string PreSharedKey { get; init; } = string.Empty;

    /// <summary>
    /// The destination IP ranges to be routed through the WireGuard server.
    /// </summary>
    [IPv4Schema]
    // ReSharper disable once InconsistentNaming
    public string[] AllowedIPs { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The endpoint of the WireGuard server including the port.
    /// </summary>
    public string Endpoint { get; init; } = string.Empty;
}
