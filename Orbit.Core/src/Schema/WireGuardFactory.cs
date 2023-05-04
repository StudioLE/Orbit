using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

public class WireGuardFactory : IFactory<WireGuard, WireGuard>
{
    private readonly IWireGuardFacade _wg;

    public WireGuardFactory(IWireGuardFacade wg)
    {
        _wg = wg;
    }

    /// <inheritdoc/>
    public WireGuard Create(WireGuard source)
    {
        WireGuard result = new();

        result.PrivateKey = source.PrivateKey.IsNullOrEmpty()
            ? _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key")
            : source.PrivateKey;

        result.PublicKey = source.PublicKey.IsNullOrEmpty()
            ? _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key")
            : source.PublicKey;

        return result;
    }
}
