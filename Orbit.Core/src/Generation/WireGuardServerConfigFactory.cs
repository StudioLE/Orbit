using Orbit.Core.Creation;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Generation;

public class WireGuardServerConfigFactory : IFactory<Network, string>
{
    private readonly IIPAddressStrategy _ip;

    public WireGuardServerConfigFactory(IIPAddressStrategy ip)
    {
        _ip = ip;
    }

    /// <inheritdoc/>
    public string Create(Network network)
    {
        return $"""
            [Interface]
            SaveConfig = true
            Address = {_ip.GetWireGuardGatewayIPv4(network)}
            Address = {_ip.GetWireGuardGatewayIPv6(network)}
            ListenPort = {network.WireGuard.Port}
            PrivateKey = {network.WireGuard.PrivateKey}
            PostUp = ufw route allow in on {network.WireGuard.Name} out on {network.Interface}
            PostUp = iptables -t nat -I POSTROUTING -o {network.Interface} -j MASQUERADE
            PostUp = ip6tables -t nat -I POSTROUTING -o {network.Interface} -j MASQUERADE
            PreDown = ufw route delete allow in on {network.WireGuard.Name} out on {network.Interface}
            PreDown = iptables -t nat -D POSTROUTING -o {network.Interface} -j MASQUERADE
            PreDown = ip6tables -t nat -D POSTROUTING -o {network.Interface} -j MASQUERADE

            """;
    }

    public static string GetFileName(Network network)
    {
        return $"{network.WireGuard.Name}.conf";
    }
}
