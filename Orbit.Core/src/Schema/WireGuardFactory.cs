using Orbit.Core.Provision;
using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

/// <summary>
/// A factory for creating <see cref="WireGuard"/> with default values.
/// </summary>
public class WireGuardFactory : IFactory<Instance, WireGuard[]>
{
    private static readonly string[] _defaultAllowedIPs =
    {
        "0.0.0.0/0",
        "::/0"
    };
    private readonly IWireGuardFacade _wg;
    private readonly IEntityProvider<Network> _networks;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardFactory"/>.
    /// </summary>
    public WireGuardFactory(IWireGuardFacade wg, IEntityProvider<Network> networks)
    {
        _wg = wg;
        _networks = networks;
    }

    /// <inheritdoc/>
    public WireGuard[] Create(Instance instance)
    {
        if (!instance.WireGuard.Any())
            instance.WireGuard = instance
                .Networks
                .Select(network => new WireGuard
                {
                    Network = network
                })
                .ToArray();
        return instance
            .WireGuard
            .Select(wg => Create(wg, instance))
            .ToArray();
    }

    private WireGuard Create(WireGuard wg, Instance instance)
    {
        WireGuard result = new()
        {
            Name = wg.Name,
            Network = wg.Network,
            PrivateKey = wg.PrivateKey,
            PublicKey = wg.PublicKey,
            Addresses = wg.Addresses,
            ServerPublicKey = wg.ServerPublicKey,
            AllowedIPs = wg.AllowedIPs,
            Endpoint = wg.Endpoint
        };

        if (result.Network.IsNullOrEmpty())
            throw new("Network must be set.");

        Network network = _networks.Get(new NetworkId(wg.Network)) ?? throw new("Failed to get the network.");

        if (result.Name.IsNullOrEmpty())
            result.Name = $"wg{network.Number}";

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");

        if (!result.Addresses.Any())
            result.Addresses = new[]
            {
                network.GetInternalIPv4(instance),
                network.GetInternalIPv6(instance)
            };

        if (result.ServerPublicKey.IsNullOrEmpty())
            result.ServerPublicKey = network.WireGuardPublicKey ?? throw new("Failed to get WireGuard public key from network.");

        if (!result.AllowedIPs.Any())
            result.AllowedIPs = _defaultAllowedIPs;

        if (result.Endpoint.IsNullOrEmpty())
            result.Endpoint = network.ExternalIPv4 + ":" + network.WireGuardPort;

        return result;
    }
}
