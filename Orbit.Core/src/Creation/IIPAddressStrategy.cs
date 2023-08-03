using Orbit.Core.Schema;

namespace Orbit.Core.Creation;

// ReSharper disable once InconsistentNaming
public interface IIPAddressStrategy
{
    public string GetInternalIPv4(Instance instance, Network network);

    public string GetInternalIPv6(Instance instance, Network network);

    public string GetInternalGatewayIPv4(Network network);

    public string GetInternalGatewayIPv6(Network network);

    public string GetWireGuardIPv4(Instance instance, Network network);

    public string GetWireGuardIPv6(Instance instance, Network network);

    public string GetWireGuardDnsIPv4(Network network);

    public string GetWireGuardDnsIPv6(Network network);

    public string GetWireGuardSubnetIPv4(Network network);

    public string GetWireGuardSubnetIPv6(Network network);
}
