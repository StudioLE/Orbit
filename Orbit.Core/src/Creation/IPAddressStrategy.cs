using Orbit.Schema;

namespace Orbit.Creation;

// ReSharper disable once InconsistentNaming
public class IPAddressStrategy : IIPAddressStrategy
{
    /// <inheritdoc/>
    public string GetInternalIPv4(IEntity entity, Network network)
    {
        return $"10.0.{network.Number}.{entity.Number}";
    }

    /// <inheritdoc/>
    public string GetInternalIPv6(IEntity entity, Network network)
    {
        return $"fc00::0:{network.Number}:{entity.Number}";
    }

    /// <inheritdoc/>
    public string? GetExternalIPv6(IEntity entity, Network network)
    {
        if(network.ExternalIPv6.EndsWith("::"))
            return $"{network.ExternalIPv6}{entity.Number}";
        return null;
    }

    /// <inheritdoc/>
    public string GetInternalGatewayIPv4(Network network)
    {
        return $"10.0.{network.Number}.1";
    }

    /// <inheritdoc/>
    public string GetInternalGatewayIPv6(Network network)
    {
        return $"fc00::0:{network.Number}:1";
    }

    /// <inheritdoc/>
    public string GetInternalDnsIPv4(Network network)
    {
        return $"10.0.{network.Number}.2";
    }

    /// <inheritdoc/>
    public string GetInternalDnsIPv6(Network network)
    {
        return $"fc00::0:{network.Number}:2";
    }

    /// <inheritdoc/>
    public string GetWireGuardIPv4(IEntity entity, Network network)
    {
        return $"10.1.{network.Number}.{entity.Number}";
    }

    /// <inheritdoc/>
    public string GetWireGuardIPv6(IEntity entity, Network network)
    {
        return $"fc00::1:{network.Number}:{entity.Number}";
    }

    /// <inheritdoc/>
    public string GetWireGuardSubnetIPv4(Network network)
    {
        return $"10.1.{network.Number}.0/24";
    }

    /// <inheritdoc/>
    public string GetWireGuardSubnetIPv6(Network network)
    {
        return $"fc00::1:{network.Number}:0/112";
    }

    public string GetWireGuardGatewayIPv4(Network network)
    {
        return $"10.1.{network.Number}.1/24";
    }

    /// <inheritdoc/>
    public string GetWireGuardGatewayIPv6(Network network)
    {
        return $"fc00::1:{network.Number}:1/112";
    }

    /// <inheritdoc/>
    public string GetWireGuardDnsIPv4(Network network)
    {
        return $"10.1.{network.Number}.2";
    }

    /// <inheritdoc/>
    public string GetWireGuardDnsIPv6(Network network)
    {
        return $"fc00::1:{network.Number}:2";
    }
}
