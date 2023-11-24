using Orbit.Core.Creation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Core.Generation;

public class WireGuardClientConfigFactory : IFactory<WireGuardClient, string>
{
    private readonly IEntityProvider<Network> _networks;
    private readonly IIPAddressStrategy _ip;

    public WireGuardClientConfigFactory(IEntityProvider<Network> networks, IIPAddressStrategy ip)
    {
        _networks = networks;
        _ip = ip;
    }

    /// <inheritdoc/>
    public string Create(WireGuardClient wg)
    {
        Network network = _networks.Get(new NetworkId(wg.Network)) ?? throw new("Failed to get network.");
        string endpoint = wg.IsExternal
            ? network.ExternalIPv4
            : _ip.GetInternalGatewayIPv4(network);
        return $"""
            [Interface]
            ListenPort = {wg.Port}
            PrivateKey = {wg.PrivateKey}
            {MultiLine("Address", wg.Addresses)}
            {MultiLine("DNS", network.WireGuard.Dns)}

            [Peer]
            PublicKey = {network.WireGuard.PublicKey}
            PreSharedKey = {wg.PreSharedKey}
            {MultiLine("AllowedIPs", wg.AllowedIPs)}
            Endpoint = {endpoint}:{network.WireGuard.Port}
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
