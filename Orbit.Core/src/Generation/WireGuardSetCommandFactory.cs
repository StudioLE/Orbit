using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class WireGuardSetCommandFactory : IFactory<WireGuard, ShellCommand>
{
    /// <inheritdoc />
    public ShellCommand Create(WireGuard wg)
    {
        // TODO: Server interface may not be wg0!
        return new()
        {
            Command = $"""
                echo {wg.PreSharedKey} | sudo wg set wg0 \
                    peer {wg.PublicKey} \
                    preshared-key /dev/fd/0 \
                    allowed-ips {wg.Addresses.Join(",")}
                """,
            ErrorMessage = "Failed to set WireGuard peer"
        };
    }
}
