using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class WireGuardSetCommandFactory : IFactory<WireGuardClient, ShellCommand>
{
    /// <inheritdoc/>
    public ShellCommand Create(WireGuardClient wg)
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
