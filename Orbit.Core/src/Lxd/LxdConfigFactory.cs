using Orbit.CloudInit;
using Orbit.Creation;
using Orbit.Provision;
using Orbit.Schema;
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
    private readonly IEntityProvider<Network> _networks;
    private readonly UserConfigFactory _userConfig;
    private readonly IIPAddressStrategy _ip;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigFactory"/>.
    /// </summary>
    public LxdConfigFactory(
        ISerializer serializer,
        IEntityProvider<Instance> instances,
        IEntityProvider<Network> networks,
        UserConfigFactory userConfig,
        IIPAddressStrategy ip)
    {
        _serializer = serializer;
        _instances = instances;
        _networks = networks;
        _userConfig = userConfig;
        _ip = ip;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        YamlNode resource = EmbeddedResourceHelpers.GetYaml("Resources/Templates/lxd-config-template.yml");
        if (resource is not YamlMappingNode yaml)
            throw new($"Expected a {nameof(YamlMappingNode)}");

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
        string networkName = instance.Networks.FirstOrDefault() ?? throw new("Instance must have a network.");
        Network network = _networks.Get(new NetworkId(networkName)) ?? throw new($"Network {networkName} not found.");
        string internalIPv4 = _ip.GetInternalIPv4(instance, network);
        string internalIPv6 = _ip.GetInternalIPv6(instance, network);
        // string externalIPv4 = _ip.GetExternalIPv4();
        string? externalIPv6 = _ip.GetExternalIPv6(instance, network);
        if (externalIPv6 is not null)
        {
            YamlMappingNode ext0 = new()
            {
                { "ipv6.address", externalIPv6 },
                { "nictype", "routed" },
                { "parent", network.Interface },
                { "type", "nic" }
            };
            devices.SetValue("ext0", ext0);
        }
        string bridgeInterface = $"br{network.Number}";
        YamlMappingNode int0 = new()
        {
            { "ipv6.address", internalIPv6 },
            { "ipv4.address", internalIPv4 },
            { "network", bridgeInterface },
            { "type", "nic" }
        };
        devices.SetValue("int0", int0);

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
