using Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Generation;

public class WireGuardSetCommandFactory : IFactory<WireGuardClient, PreparedShellCommand>
{
    /// <inheritdoc/>
    public PreparedShellCommand Create(WireGuardClient wg)
    {
        return new()
        {
            Command = $"""
                echo {wg.PreSharedKey} | sudo wg set {wg.Name} \
                    peer {wg.PublicKey} \
                    preshared-key /dev/fd/0 \
                    allowed-ips {wg.Addresses.Join(",")}
                """,
            ErrorMessage = "Failed to set WireGuard peer"
        };
    }
}
