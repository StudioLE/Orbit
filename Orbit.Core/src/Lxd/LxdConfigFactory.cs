using Orbit.CloudInit;
using Orbit.Instances;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Yaml;
using StudioLE.Patterns;

namespace Orbit.Lxd;

public class LxdConfigFactory : IFactory<Instance, string>
{
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly UserConfigFactory _userConfig;
    private readonly NetplanFactory _netplanFactory;
    private readonly ExternalInterfaceFactory _externalInterfaceFactory;
    private readonly InternalInterfaceFactory _internalInterfaceFactory;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigFactory"/>.
    /// </summary>
    public LxdConfigFactory(
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        UserConfigFactory userConfig,
        NetplanFactory netplanFactory,
        ExternalInterfaceFactory externalInterfaceFactory,
        InternalInterfaceFactory internalInterfaceFactory)
    {
        _instances = instances;
        _userConfig = userConfig;
        _netplanFactory = netplanFactory;
        _externalInterfaceFactory = externalInterfaceFactory;
        _internalInterfaceFactory = internalInterfaceFactory;
        _servers = servers;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        Server server = _servers.Get(instance.Server) ?? throw new($"Server not found: {instance.Server}.");
        string? userConfig = _instances.GetArtifact(instance.Name, UserConfigActivity.FileName);
        if (userConfig is null)
            userConfig = _userConfig.Create(instance);
        Interface serverNic = server
                                  .Interfaces
                                  .FirstOrNull(x => x.Type == NetworkType.Nic)
                              ?? throw new($"NIC not found for server: {server.Name}.");
        Interface serverBridge = server
                                  .Interfaces
                                  .FirstOrNull(x => x.Type == NetworkType.Bridge)
                              ?? throw new($"Bridge not found for server: {server.Name}.");
        string networkConfig = _netplanFactory.Create(instance);
        return $"""
            type: virtual-machine
            name: {instance.Name}
            devices:
              {_externalInterfaceFactory.GetName(server)}:
                ipv6.address: '{_externalInterfaceFactory.GetIPv6Address(instance, serverNic)}'
                nictype: routed
                parent: {serverNic.Name}
                type: nic
                name: {_externalInterfaceFactory.GetName(server)}
                hwaddr: {_externalInterfaceFactory.GetMacAddress(instance, server)}
              {_internalInterfaceFactory.GetName(server)}:
                ipv4.address: {_internalInterfaceFactory.GetIPv4Address(instance, server)}
                ipv6.address: '{_internalInterfaceFactory.GetIPv6Address(instance, server)}'
                network: {serverBridge.Name}
                type: nic
                name: {_internalInterfaceFactory.GetName(server)}
                hwaddr: {_internalInterfaceFactory.GetMacAddress(instance, server)}
            config:
              limits.cpu: '{instance.Hardware.Cpus}'
              limits.memory: {instance.Hardware.Memory}GB
              cloud-init.network-config: |
            {networkConfig.Indent(2).TrimEnd()}
              cloud-init.user-data: |
            {userConfig.Indent(2).TrimEnd()}

            """;
    }
}
