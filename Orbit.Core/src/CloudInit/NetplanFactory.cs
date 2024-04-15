using Orbit.Instances;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
using Orbit.Utils.Yaml;
using StudioLE.Patterns;

namespace Orbit.CloudInit;

/// <summary>
/// Create a Netplan configuration for an <see cref="Instance"/> with a external routed nic interface using <see cref="ExternalInterfaceFactory"/> and an internal bridge interface using <see cref="InternalInterfaceFactory"/>.
/// </summary>
/// <seealso href="https://netplan.readthedocs.io/en/stable/netplan-yaml/"/>
public class NetplanFactory : IFactory<Instance, string>
{
    private readonly IEntityProvider<Server> _servers;
    private readonly ExternalInterfaceFactory _externalInterfaceFactory;
    private readonly InternalInterfaceFactory _internalInterfaceFactory;

    /// <summary>
    /// DI constructor for <see cref="NetplanFactory"/>.
    /// </summary>
    public NetplanFactory(
        IEntityProvider<Server> servers,
        ExternalInterfaceFactory externalInterfaceFactory,
        InternalInterfaceFactory internalInterfaceFactory)
    {
        _servers = servers;
        _externalInterfaceFactory = externalInterfaceFactory;
        _internalInterfaceFactory = internalInterfaceFactory;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        Server server = _servers.Get(instance.Server) ?? throw new($"Server not found: {instance.Server}.");
        Interface nic = server
                            .Interfaces
                            .FirstOrNull(x => x.Type == NetworkType.Nic)
                        ?? throw new($"NIC not found for server: {instance.Server}.");
        return $"""
            network:
              version: 2
              ethernets:
                {_externalInterfaceFactory.GetName(server)}:
                  dhcp4: no
                  match:
                    macaddress: {_externalInterfaceFactory.GetMacAddress(instance, server)}
                  addresses:
                  - {_externalInterfaceFactory.GetIPv6AddressWithCidr(instance, nic).AsYamlString()}
                  nameservers:
                    addresses:{nic.Dns.AsYamlSequence(4)}
                  routes:
                  - to: default
                    via: '{nic.Gateways.Where(IPHelpers.IsIPv6).First()}'
                    metric: 10
                    on-link: true
                {_internalInterfaceFactory.GetName(server)}:
                  dhcp4: no
                  match:
                    macaddress: {_internalInterfaceFactory.GetMacAddress(instance, server)}
                  addresses:
                  - {_internalInterfaceFactory.GetIPv4AddressWithCidr(instance, server)}
                  - {_internalInterfaceFactory.GetIPv6AddressWithCidr(instance, server).AsYamlString()}
                  nameservers:
                    addresses:
                    - {_internalInterfaceFactory.GetIPv4Dns(server)}
                    - {_internalInterfaceFactory.GetIPv6Dns(server).AsYamlString()}
                  routes:
                  - to: default
                    via: {_internalInterfaceFactory.GetIPv4Gateway(server)}
                    metric: 20
                  - to: default
                    via: {_internalInterfaceFactory.GetIPv6Gateway(server).AsYamlString()}
                    metric: 20

            """;
    }
}
