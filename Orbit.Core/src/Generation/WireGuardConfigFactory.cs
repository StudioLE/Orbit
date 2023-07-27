using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class WireGuardConfigFactory : IFactory<WireGuard, string>
{
    /// <inheritdoc/>
    public string Create(WireGuard wg)
    {
        string addresses = wg
            .Addresses
            .Select(address => "Address = " + address)
            .Join();
        return $"""
            [Interface]
            PrivateKey = {wg.PrivateKey}
            {addresses}
            DNS = {wg.Dns}

            [Peer]
            PublicKey = {wg.ServerPublicKey}
            PreSharedKey = {wg.PreSharedKey}
            AllowedIPs = {wg.AllowedIPs.Join(", ")}
            Endpoint = {wg.Endpoint}
            PersistentKeepalive = 25

            """;
    }
}
