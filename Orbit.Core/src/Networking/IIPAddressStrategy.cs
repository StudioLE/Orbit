using Orbit.Schema;

namespace Orbit.Networking;

// ReSharper disable once InconsistentNaming
public interface IIPAddressStrategy
{
    public string GetInternalIPv4(IEntity instance, Network network);

    public string GetInternalIPv6(IEntity instance, Network network);

    public string? GetExternalIPv6(IEntity instance, Network network);

    public string GetInternalGatewayIPv4(Network network);

    public string GetInternalDnsIPv6(Network network);

    public string GetInternalDnsIPv4(Network network);

    public string GetInternalGatewayIPv6(Network network);

    public string GetWireGuardIPv4(IEntity instance, Network network);

    public string GetWireGuardIPv6(IEntity instance, Network network);

    public string GetWireGuardSubnetIPv4(Network network);

    public string GetWireGuardSubnetIPv6(Network network);

    public string GetWireGuardGatewayIPv4(Network network);

    public string GetWireGuardGatewayIPv6(Network network);

    public string GetWireGuardDnsIPv4(Network network);

    public string GetWireGuardDnsIPv6(Network network);
}
