using Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.WireGuard;

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
