using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

public sealed class WireGuard
{
    [Base64Schema]
    public string PrivateKey { get; set; } = string.Empty;

    [Base64Schema]
    public string PublicKey { get; set; } = string.Empty;

    [IPSchema]
    public string[] Addresses { get; set; } = Array.Empty<string>();

    [Base64Schema]
    public string ServerPublicKey { get; set; } = string.Empty;

    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string AllowedIPs { get; set; } = string.Empty;

    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string Endpoint { get; set; } = string.Empty;
}
