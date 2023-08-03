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
        return $"""
            [Interface]
            PrivateKey = {wg.PrivateKey}
            {MultiLine("Address", wg.Addresses)}
            {MultiLine("DNS", network.WireGuard.Dns)}

            [Peer]
            PublicKey = {network.WireGuard.PublicKey}
            PreSharedKey = {wg.PreSharedKey}
            {MultiLine("AllowedIPs", wg.AllowedIPs)}
            Endpoint = {network.ExternalIPv4}:{network.WireGuard.Port}
            PersistentKeepAlive = 25

            """;
    }

    private static string MultiLine(string key, string[] values)
    {
        return values
            .Select(value => $"{key} = {value}")
            .Join();
    }
}
