using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class WireGuardClientConfigFactory : IFactory<WireGuardClient, string>
{
    private readonly IEntityProvider<Network> _networks;

    public WireGuardClientConfigFactory(IEntityProvider<Network> networks)
    {
        _networks = networks;
    }

    /// <inheritdoc/>
    public string Create(WireGuardClient wg)
    {
        Network network = _networks.Get(new NetworkId(wg.Network)) ?? throw new("Failed to get network.");
        string addresses = wg
            .Addresses
            .Select(address => "Address = " + address)
            .Join();
        return $"""
            [Interface]
            PrivateKey = {wg.PrivateKey}
            {addresses}
            DNS = {network.WireGuard.Dns.Join(", ")}

            [Peer]
            PublicKey = {network.WireGuard.PublicKey}
            PreSharedKey = {wg.PreSharedKey}
            AllowedIPs = {wg.AllowedIPs.Join(", ")}
            Endpoint = {network.ExternalIPv4}:{network.WireGuard.Port}
            PersistentKeepalive = 25

            """;
    }
}
