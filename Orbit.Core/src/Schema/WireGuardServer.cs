using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the WireGuard interface of an instance.
/// </summary>
public readonly record struct WireGuardServer()
{
    /// <summary>
    /// The name of the WireGuard interface.
    /// </summary>
    [NameSchema]
    public string Name { get; init; } = string.Empty;

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
}
