using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class WireGuardSetCommandFactory : IFactory<WireGuard, string>
{
    /// <inheritdoc />
    public string Create(WireGuard wg)
    {
        // TODO: Server interface may not be wg0!
        return $"sudo wg set wg0 peer {wg.PublicKey} allowed-ips {wg.Addresses.Join(",")}";
    }
}
