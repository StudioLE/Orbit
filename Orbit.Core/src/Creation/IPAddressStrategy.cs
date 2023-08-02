using Orbit.Core.Schema;

namespace Orbit.Core.Creation;

// ReSharper disable once InconsistentNaming
public class IPAddressStrategy : IIPAddressStrategy
{
    public string GetInternalIPv4(Instance instance, Network network)
    {
        return $"10.0.{network.Number}.{instance.Number}";
    }

    public string GetInternalIPv6(Instance instance, Network network)
    {
        return $"fc00::0:{network.Number}:{instance.Number}";
    }

    public string GetInternalGatewayIPv4(Network network)
    {
        return $"10.0.{network.Number}.1";
    }

    public string GetInternalGatewayIPv6(Network network)
    {
        return $"fc00::0:{network.Number}:1";
    }

    public string GetWireGuardIPv4(Instance instance, Network network)
    {
        return $"10.1.{network.Number}.{instance.Number}";
    }

    public string GetWireGuardIPv6(Instance instance, Network network)
    {
        return $"fc00::1:{network.Number}:{instance.Number}";
    }

    public string GetWireGuardDnsIPv4(Network network)
    {
        return $"10.1.{network.Number}.2";
    }

    /// <inheritdoc />
    public string GetWireGuardDnsIPv6(Network network)
    {
        return $"fc00::1:{network.Number}:2";
    }

    public string GetWireGuardGatewayIPv4(Network network)
    {
        return $"10.1.{network.Number}.1";
    }

    public string GetWireGuardGatewayIPv6(Network network)
    {
        return $"fc00::1:{network.Number}:1";
    }
}
