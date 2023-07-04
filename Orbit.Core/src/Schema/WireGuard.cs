using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for the WireGuard peer.
/// </summary>
public sealed class WireGuard
{
    /// <summary>
    /// The private key of the WireGuard peer.
    /// </summary>
    [Base64Schema]
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// The public key of the WireGuard peer.
    /// </summary>
    [Base64Schema]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// The addresses of the WireGuard peer.
    /// </summary>
    [IPSchema]
    public string[] Addresses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The public key of the WireGuard server.
    /// </summary>
    [Base64Schema]
    public string ServerPublicKey { get; set; } = string.Empty;

    /// <summary>
    /// The destination IP ranges to be routed through the WireGuard server.
    /// </summary>
    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string[] AllowedIPs { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The address of the WireGuard server.
    /// </summary>
    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string Endpoint { get; set; } = string.Empty;
}
