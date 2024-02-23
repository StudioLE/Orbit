using Orbit.CloudInit;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
using StudioLE.Patterns;
using StudioLE.Serialization;
using StudioLE.Serialization.Yaml;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Orbit.Lxd;

public class LxdConfigFactory : IFactory<Instance, string>
{
    private readonly ISerializer _serializer;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly UserConfigFactory _userConfig;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigFactory"/>.
    /// </summary>
    public LxdConfigFactory(
        ISerializer serializer,
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        UserConfigFactory userConfig)
    {
        _serializer = serializer;
        _instances = instances;
        _userConfig = userConfig;
        _servers = servers;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        YamlNode resource = EmbeddedResourceHelpers.GetYaml("Resources/Templates/lxd-config-template.yml");
        if (resource is not YamlMappingNode yaml)
            throw new($"Expected a {nameof(YamlMappingNode)}");

        // TODO: Rewrite to use multiline interpolated strings

        // Name
        yaml.SetValue("name", instance.Name, false);

        // Devices
        YamlMappingNode? devices = yaml.GetValue<YamlMappingNode>("config");
        if (devices is null)
        {
            devices = new();
            yaml.SetValue("devices", devices);
        }

        // Root disk
        YamlMappingNode rootDisk = new()
        {
            { "size", $"{instance.Hardware.Disk}GB" }
        };
        devices.SetValue("root", rootDisk);

        // Networks
        Server server = _servers.Get(new ServerId(instance.Server)) ?? throw new($"Server not found: {instance.Server}.");
        Interface? routedNicQuery = instance.Interfaces.FirstOrNull(x => x.Type == NetworkType.RoutedNic);
        if (routedNicQuery is Interface routedNic)
        {
            string routedIPv6 = routedNic.Addresses.FirstOrDefault(IPHelpers.IsIPv6) ?? throw new("No IPv6 found.");
            Interface serverNic = server.Interfaces.FirstOrNull(x => x.Type == NetworkType.Nic) ?? throw new($"NIC not found for server: {server.Name}.");
            YamlMappingNode routedNicDevice = new()
            {
                { "ipv6.address", routedIPv6 },
                { "nictype", "routed" },
                { "parent", serverNic.Name },
                { "type", "nic" }
            };
            devices.SetValue(routedNic.Name, routedNicDevice);
        }
        Interface bridge = instance.Interfaces.FirstOrNull(x => x.Type == NetworkType.Bridge) ?? throw new($"Bridge not found for instance: {instance.Name}.");
        string ipv4 = bridge.Addresses.FirstOrDefault(IPHelpers.IsIPv4) ?? string.Empty;
        string ipv6 = bridge.Addresses.FirstOrDefault(IPHelpers.IsIPv6) ?? string.Empty;
        // TODO: The server may have more than one bridge. If so how do we know which one to use?
        string bridgeParent = server.Interfaces.FirstOrNull(x => x.Type == NetworkType.Bridge)?.Name ?? throw new($"Bridge not found for server: {server.Name}.");
        ipv4 = IPHelpers.RemoveCidr(ipv4);
        ipv6 = IPHelpers.RemoveCidr(ipv6);
        YamlMappingNode bridgeDevice = new()
        {
            { "ipv4.address", ipv4 },
            { "ipv6.address", ipv6 },
            { "network", bridgeParent },
            { "type", "nic" }
        };
        devices.SetValue(bridge.Name, bridgeDevice);

        // Config
        YamlMappingNode? config = yaml.GetValue<YamlMappingNode>("config");
        if (config is null)
        {
            config = new();
            yaml.SetValue("config", config);
        }

        // See: https://documentation.ubuntu.com/lxd/en/latest/reference/instance_options

        // Hardware
        config.SetValue("limits.cpu", $"{instance.Hardware.Cpus}");
        config.SetValue("limits.memory", $"{instance.Hardware.Memory}GB");

        // Cloud Init
        string? userConfig = _instances.GetResource(new InstanceId(instance.Name), GenerateUserConfig.FileName);
        if (userConfig is null)
            userConfig = _userConfig.Create(instance);
        config.SetValue("cloud-init.user-data", new YamlScalarNode(userConfig)
        {
            Style = ScalarStyle.Literal
        });
        // TODO: Cloud init Network config

        // Serialize
        string output = _serializer.Serialize(yaml);
        return output;
    }
}
