using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Creation;

/// <summary>
/// A factory for creating <see cref="WireGuardServer"/> with default values.
/// </summary>
public class WireGuardServerFactory : IFactory<Network, WireGuardServer>
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
    public WireGuardServer Create(Network network)
    {
        WireGuardServer result = new()
        {
            Name = network.WireGuard.Name,
            PrivateKey = network.WireGuard.PrivateKey,
            PublicKey = network.WireGuard.PublicKey
        };

        if (result.Name.IsNullOrEmpty())
            result.Name = $"wg{network.Number}";

        if (result.Port == default)
            result.Port = 51820;

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");

        return result;
    }
}
