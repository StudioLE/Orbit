using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils;

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
    public string HostPublicKey { get; set; } = string.Empty;

    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string AllowedIPs { get; set; } = string.Empty;

    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string Endpoint { get; set; } = string.Empty;

    public void Review()
    {
        WireGuardFacade wg = new();

        if (PrivateKey.IsNullOrEmpty())
            PrivateKey = wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (PublicKey.IsNullOrEmpty())
            PublicKey = wg.GeneratePublicKey(PrivateKey) ?? throw new("Failed to generate WireGuard public key");
    }
}
