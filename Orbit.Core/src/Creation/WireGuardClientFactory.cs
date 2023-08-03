using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Creation;

/// <summary>
/// A factory for creating <see cref="WireGuardClient"/> with default values.
/// </summary>
public class WireGuardClientFactory : IFactory<Instance, WireGuardClient[]>
{
    private readonly IWireGuardFacade _wg;
    private readonly IEntityProvider<Network> _networks;
    private readonly IIPAddressStrategy _ip;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardClientFactory"/>.
    /// </summary>
    public WireGuardClientFactory(IWireGuardFacade wg, IEntityProvider<Network> networks, IIPAddressStrategy ip)
    {
        _wg = wg;
        _networks = networks;
        _ip = ip;
    }

    /// <inheritdoc/>
    public WireGuardClient[] Create(Instance instance)
    {
        if (!instance.WireGuard.Any())
            instance.WireGuard = instance
                .Networks
                .Select(network => new WireGuardClient
                {
                    Network = network
                })
                .ToArray();
        return instance
            .WireGuard
            .Select(wg => Create(wg, instance))
            .ToArray();
    }

    private WireGuardClient Create(WireGuardClient wg, Instance instance)
    {
        WireGuardClient result = new()
        {
            Name = wg.Name,
            Network = wg.Network,
            PrivateKey = wg.PrivateKey,
            PublicKey = wg.PublicKey,
            PreSharedKey = wg.PreSharedKey,
            Addresses = wg.Addresses,
            AllowedIPs = wg.AllowedIPs
        };

        if (result.Network.IsNullOrEmpty())
            throw new("Network must be set.");

        Network network = _networks.Get(new NetworkId(wg.Network)) ?? throw new("Failed to get the network.");

        if (result.Name.IsNullOrEmpty())
            result.Name = $"wg{network.Number}";

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");

        if (result.PreSharedKey.IsNullOrEmpty())
            result.PreSharedKey = _wg.GeneratePreSharedKey() ?? throw new("Failed to generate WireGuard pre-shared key");

        if (!result.Addresses.Any())
            result.Addresses = new[]
            {
                _ip.GetWireGuardIPv4(instance, network),
                _ip.GetWireGuardIPv6(instance, network)
            };

        if (!result.AllowedIPs.Any())
            result.AllowedIPs = new[]
            {
                _ip.GetWireGuardSubnetIPv4(network),
                _ip.GetWireGuardSubnetIPv6(network)
            };

        return result;
    }
}
