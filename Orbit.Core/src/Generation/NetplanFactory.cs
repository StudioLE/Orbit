using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Generation;

public class NetplanFactory : IFactory<Instance, string>
{
    private readonly IEntityProvider<Network> _networks;

    public NetplanFactory(IEntityProvider<Network> networks)
    {
        _networks = networks;
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
                  - {network.GetInternalIPv4(instance)}/24
                  - {network.GetInternalIPv6(instance)}/112
                  routes:
                  - to: default
                    via: 10.0.{network.Number}.1
                    metric: 50
                  - to: default
                    via: fc00::0.{network.Number}.1
                    metric: 50

            """;
    }
}
