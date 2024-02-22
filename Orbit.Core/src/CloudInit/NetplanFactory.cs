using Orbit.Networking;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Patterns;

namespace Orbit.CloudInit;

public class NetplanFactory : IFactory<Instance, string>
{
    private readonly IEntityProvider<Network> _networks;
    private readonly IIPAddressStrategy _ip;

    public NetplanFactory(IEntityProvider<Network> networks, IIPAddressStrategy ip)
    {
        _networks = networks;
        _ip = ip;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        string networkName = instance.Networks.FirstOrDefault() ?? throw new("Instance must have a network.");
        Network network = _networks.Get(new NetworkId(networkName)) ?? throw new($"Network {networkName} not found.");
        return $"""
            network:
              version: 2
              ethernets:
                extra0:
                  dhcp4: no
                  match:
                    macaddress: {instance.MacAddress}
                  addresses:
                  - {_ip.GetInternalIPv4(instance, network)}/24
                  - {_ip.GetInternalIPv6(instance, network)}/112
                  nameservers:
                    addresses:
                    - {_ip.GetInternalDnsIPv4(network)}
                    - {_ip.GetInternalDnsIPv6(network)}
                  routes:
                  - to: default
                    via: {_ip.GetInternalGatewayIPv4(network)}
                    metric: 50
                  - to: default
                    via: {_ip.GetInternalGatewayIPv6(network)}
                    metric: 50

            """;
    }
}
