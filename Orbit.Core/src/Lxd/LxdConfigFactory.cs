using Orbit.CloudInit;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
using Orbit.Utils.Yaml;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Lxd;

public class LxdConfigFactory : IFactory<Instance, string>
{
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly UserConfigFactory _userConfig;
    private readonly NetplanFactory _netplanFactory;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigFactory"/>.
    /// </summary>
    public LxdConfigFactory(
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        UserConfigFactory userConfig,
        NetplanFactory netplanFactory)
    {
        _instances = instances;
        _userConfig = userConfig;
        _netplanFactory = netplanFactory;
        _servers = servers;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        Server server = _servers.Get(instance.Server) ?? throw new($"Server not found: {instance.Server}.");
        string interfaces = instance
            .Interfaces
            .OrderBy(x => x.Name)
            .Select(x => CreateInterface(x, server))
            .Join();
        string? userConfig = _instances.GetArtifact(instance.Name, GenerateUserConfig.FileName);
        if (userConfig is null)
            userConfig = _userConfig.Create(instance);
        string networkConfig = _netplanFactory.Create(instance);
        return $"""
            type: virtual-machine
            name: example-instance
            devices:
              root:
                size: 20GB
            {interfaces}
            config:
              limits.cpu: '2'
              limits.memory: 2GB
              cloud-init.network-config: |
            {networkConfig.Indent(2).TrimEnd()}
              cloud-init.user-data: |
            {userConfig.Indent(2).TrimEnd()}

            """;
    }

    private static string CreateInterface(Interface iface, Server server)
    {
        return iface.Type switch
        {
            NetworkType.Bridge => CreateBridge(iface, server),
            NetworkType.RoutedNic => CreateRoutedNic(iface, server),
            _ => throw new NotSupportedException($"Network type {iface.Type} is not supported.")
        };
    }

    private static string CreateRoutedNic(Interface iface, Server server)
    {
        Interface serverNic = server
                                  .Interfaces
                                  .FirstOrNull(x => x.Type == NetworkType.Nic)
                              ?? throw new($"NIC not found for server: {server.Name}.");
        string ipv6 = iface
                          .Addresses
                          .FirstOrDefault(IPHelpers.IsIPv6)
                      ?? string.Empty;
        ipv6 = IPHelpers.RemoveCidr(ipv6);
        string output = $"""
              {iface.Name}:
                ipv6.address: '{ipv6}'
                nictype: routed
                parent: {serverNic.Name}
                type: nic
                name: {iface.Name}
                hwaddr: {iface.MacAddress}
            """;
        return output;
    }

    private static string CreateBridge(Interface iface, Server server)
    {
        Interface serverInterface = server
                                        .Interfaces
                                        .FirstOrNull(x => x.Type == NetworkType.Bridge)
                                    ?? throw new($"Bridge not found for server: {server.Name}.");
        string ipv4 = iface
                          .Addresses
                          .FirstOrDefault(IPHelpers.IsIPv4)
                      ?? string.Empty;
        string ipv6 = iface
                          .Addresses
                          .FirstOrDefault(IPHelpers.IsIPv6)
                      ?? string.Empty;
        ipv4 = IPHelpers.RemoveCidr(ipv4);
        ipv6 = IPHelpers.RemoveCidr(ipv6);
        string output = $"""
              {iface.Name}:
                ipv4.address: {ipv4}
                ipv6.address: '{ipv6}'
                network: {serverInterface.Name}
                type: nic
                name: {iface.Name}
                hwaddr: {iface.MacAddress}
            """;
        return output;
    }
}
