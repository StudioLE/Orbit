using Orbit.Core.Provision;
using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

/// <summary>
/// A factory for creating <see cref="WireGuard"/> with default values.
/// </summary>
public class WireGuardFactory : IFactory<Instance, WireGuard>
{
    private readonly IWireGuardFacade _wg;
    private readonly IEntityProvider<Network> _networks;
    private static readonly string[] _defaultAllowedIPs =
    {
        "0.0.0.0/0",
        "::/0"
    };

    /// <summary>
    /// The DI constructor for <see cref="WireGuardFactory"/>.
    /// </summary>
    public WireGuardFactory(IWireGuardFacade wg, IEntityProvider<Network> networks)
    {
        _wg = wg;
        _networks = networks;
    }

    /// <inheritdoc/>
    public WireGuard Create(Instance instance)
    {
        WireGuard result = new()
        {
            PrivateKey = instance.WireGuard.PrivateKey,
            PublicKey = instance.WireGuard.PublicKey,
            Addresses = instance.WireGuard.Addresses,
            ServerPublicKey = instance.WireGuard.ServerPublicKey,
            AllowedIPs = instance.WireGuard.AllowedIPs,
            Endpoint = instance.WireGuard.Endpoint
        };

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if(result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");

        Network network = _networks.Get(new NetworkId(instance.Network)) ?? throw new("Failed to get the network.");

        if (!result.Addresses.Any())
            result.Addresses = new[]
            {
                network.GetInternalIPv4(instance),
                network.GetInternalIPv6(instance)
            };

        if(result.ServerPublicKey.IsNullOrEmpty())
            result.ServerPublicKey = network.WireGuardPublicKey ?? throw new("Failed to get WireGuard public key from network.");

        if (!result.AllowedIPs.Any())
            result.AllowedIPs = _defaultAllowedIPs;

        if (result.Endpoint.IsNullOrEmpty())
            result.Endpoint = network.ExternalIPv4 + ":" + network.WireGuardPort;

        return result;
    }
}
