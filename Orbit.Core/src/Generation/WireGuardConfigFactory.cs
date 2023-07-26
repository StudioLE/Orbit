using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class WireGuardConfigFactory : IFactory<WireGuard, string>
{
    /// <inheritdoc/>
    public string Create(WireGuard wg)
    {
        List<string> lines = new()
        {
            $"""
            [Interface]
            PrivateKey = {wg.PrivateKey}
            """
        };
        foreach (string address in wg.Addresses)
            lines.Add($"Address = {address}");
        lines.Add($"""
            [Peer]
            PublicKey = {wg.ServerPublicKey}
            AllowedIPs = {wg.AllowedIPs.Join(", ")}
            Endpoint = {wg.Endpoint}
            """);
        return lines.Join();
    }
}
