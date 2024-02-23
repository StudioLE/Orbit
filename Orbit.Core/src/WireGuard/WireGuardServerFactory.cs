using Orbit.Schema;
using Orbit.Utils;
using StudioLE.Patterns;

namespace Orbit.WireGuard;

/// <summary>
/// A factory for creating <see cref="WireGuardServer"/> with default values.
/// </summary>
public class WireGuardServerFactory : IFactory<Server, WireGuardServer>
{
    private readonly IWireGuardFacade _wg;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardServerFactory"/>.
    /// </summary>
    public WireGuardServerFactory(IWireGuardFacade wg)
    {
        _wg = wg;
    }

    /// <inheritdoc/>
    public WireGuardServer Create(Server server)
    {
        WireGuardServer wg = server.WireGuard;
        if (wg.Name.IsDefault())
            wg = wg with
            {
                Name = $"wg{server.Number}"
            };
        if(wg.Port.IsDefault())
            wg = wg with
            {
                Port = 61000 + server.Number
            };
        if(wg.PrivateKey.IsDefault())
            wg = wg with
            {
                PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key")
            };
        if(wg.PublicKey.IsDefault())
            wg = wg with
            {
                PublicKey = _wg.GeneratePublicKey(wg.PrivateKey) ?? throw new("Failed to generate WireGuard public key")
            };
        return wg;
    }
}
