using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class WireGuardConfigFactory : IFactory<WireGuardClient, string>
{
    private readonly IEntityProvider<Network> _networks;

    public WireGuardConfigFactory(IEntityProvider<Network> networks)
    {
        _networks = networks;
    }

    /// <inheritdoc/>
    public string Create(WireGuardClient client)
    {
        Network network = _networks.Get(new NetworkId(client.Network)) ?? throw new("Failed to get network.");
        string addresses = client
            .Addresses
            .Select(address => "Address = " + address)
            .Join();
        return $"""
            [Interface]
            PrivateKey = {client.PrivateKey}
            {addresses}
            DNS = {network.Dns}

            [Peer]
            PublicKey = {network.WireGuard.PublicKey}
            PreSharedKey = {client.PreSharedKey}
            AllowedIPs = {client.AllowedIPs.Join(", ")}
            Endpoint = {network.ExternalIPv4}:{network.WireGuard.Port}
            PersistentKeepalive = 25

            """;
    }
}
