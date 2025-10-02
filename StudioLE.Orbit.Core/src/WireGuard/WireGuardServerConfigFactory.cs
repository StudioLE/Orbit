using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Utils;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace StudioLE.Orbit.WireGuard;

/// <summary>
/// Create a WireGuard configuration file for a <see cref="Server"/>.
/// </summary>
public class WireGuardServerConfigFactory : IFactory<Server, string>
{
    /// <inheritdoc/>
    public string Create(Server server)
    {
        Interface nic = server
                            .Interfaces
                            .FirstOrNull(x => x.Type == NetworkType.Nic)
                        ?? throw new($"NIC not found for server: {server.Name}.");
        Interface bridge = server
                               .Interfaces
                               .FirstOrNull(x => x.Type == NetworkType.Bridge)
                           ?? throw new($"Bridge not found for server: {server.Name}.");
        return $"""
            [Interface]
            SaveConfig = true
            {MultiLine("Address", bridge.Addresses)}
            ListenPort = {server.WireGuard.Port}
            PrivateKey = {server.WireGuard.PrivateKey}
            PostUp = ufw route allow in on {server.WireGuard.Name} out on {nic.Name}
            PostUp = iptables -t nat -I POSTROUTING -o {nic.Name} -j MASQUERADE
            PostUp = ip6tables -t nat -I POSTROUTING -o {nic.Name} -j MASQUERADE
            PreDown = ufw route delete allow in on {server.WireGuard.Name} out on {nic.Name}
            PreDown = iptables -t nat -D POSTROUTING -o {nic.Name} -j MASQUERADE
            PreDown = ip6tables -t nat -D POSTROUTING -o {nic.Name} -j MASQUERADE
            """;
    }

    private static string MultiLine(string key, IEnumerable<string> values)
    {
        return values
            .Select(value => $"{key} = {value}")
            .Join();
    }
}
