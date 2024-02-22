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
        WireGuardServer result = new()
        {
            Name = server.WireGuard.Name,
            Port = server.WireGuard.Port,
            PrivateKey = server.WireGuard.PrivateKey,
            PublicKey = server.WireGuard.PublicKey
        };

        if (result.Name.IsNullOrEmpty())
            result.Name = $"wg{server.Number}";

        if (result.Port == default)
            result.Port = 61000 + server.Number;

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");

        return result;
    }
}
