using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.Networking;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.WireGuard;

public class WireGuardClientConfigFactory : IFactory<WireGuardClient, string>
{
    private readonly IEntityProvider<Server> _servers;

    public WireGuardClientConfigFactory(IEntityProvider<Server> servers)
    {
        _servers = servers;
    }

    /// <inheritdoc/>
    public string Create(WireGuardClient wg)
    {
        Server server = _servers.Get(wg.Interface.Server) ?? throw new($"Server not found: {wg.Interface.Server}.");
        return $"""
            [Interface]
            ListenPort = {wg.Port}
            PrivateKey = {wg.PrivateKey}
            {MultiLine("Address", wg.Interface.Addresses.Select(IPHelpers.RemoveCidr))}
            {MultiLine("DNS", wg.Interface.Dns)}

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
