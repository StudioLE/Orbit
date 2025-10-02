using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Servers;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace StudioLE.Orbit.WireGuard;

/// <summary>
/// Create a WireGuard configuration file for a <see cref="Client"/> or <see cref="Instance"/>.
/// </summary>
public class WireGuardClientConfigFactory : IFactory<WireGuardClient, Task<string>>
{
    private readonly ServerProvider _servers;
    private readonly WireGuardInterfaceFactory _wgInterfaceFactory;

    /// <summary>
    /// DI constructor for <see cref="WireGuardClientConfigFactory"/>.
    /// </summary>
    public WireGuardClientConfigFactory(ServerProvider servers, WireGuardInterfaceFactory wgInterfaceFactory)
    {
        _servers = servers;
        _wgInterfaceFactory = wgInterfaceFactory;
    }

    /// <inheritdoc/>
    public async Task<string> Create(WireGuardClient wg)
    {
        Server server = await _servers.Get(wg.Server) ?? throw new($"Server not found: {wg.Server}.");
        return $"""
            [Interface]
            ListenPort = {wg.Port}
            PrivateKey = {wg.PrivateKey}
            {MultiLine("Address", wg.Addresses)}
            DNS = {_wgInterfaceFactory.GetIPv4Dns(server)}
            DNS = {_wgInterfaceFactory.GetIPv6Dns(server)}

            [Peer]
            PublicKey = {server.WireGuard.PublicKey}
            PreSharedKey = {wg.PreSharedKey}
            {MultiLine("AllowedIPs", wg.AllowedIPs)}
            Endpoint = {wg.Endpoint}
            PersistentKeepAlive = 25
            """;
    }

    private static string MultiLine(string key, IEnumerable<string> values)
    {
        return values
            .Select(value => $"{key} = {value}")
            .Join();
    }
}
