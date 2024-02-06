using Orbit.Provision;
using Orbit.Schema;
using Orbit.Shell;
using Orbit.Utils;
using StudioLE.Patterns;

namespace Orbit.Creation;

/// <summary>
/// A factory for creating <see cref="WireGuardClient"/> with default values.
/// </summary>
public class WireGuardClientFactory : IFactory<IHasWireGuardClient, WireGuardClient[]>
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
    public WireGuardClient[] Create(IHasWireGuardClient instance)
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

    private WireGuardClient Create(WireGuardClient wg, IHasWireGuardClient instance)
    {
        WireGuardClient result = new()
        {
            Name = wg.Name,
            Network = wg.Network,
            IsExternal = wg.IsExternal,
            Port = wg.Port,
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

        result.IsExternal = instance is not Instance actualInstance
                            || actualInstance.Server != network.Server;

        if (result.Port == default)
            result.Port = 61000 + network.Number;

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
