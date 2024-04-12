using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Patterns;
using StudioLE.Serialization;
using StudioLE.Serialization.Yaml;
using YamlDotNet.RepresentationModel;

namespace Orbit.CloudInit;

public class NetplanFactory : IFactory<Instance, string>
{
    private readonly ISerializer _serializer;

    public NetplanFactory(ISerializer serializer)
    {
        _serializer = serializer;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        YamlNode resource = EmbeddedResourceHelpers.GetYaml("Resources/Templates/netplan-config-template.yml");
        if (resource is not YamlMappingNode yaml)
            throw new($"Expected a {nameof(YamlMappingNode)}");

        // Network
        YamlMappingNode root = yaml.GetValue<YamlMappingNode>("network") ?? throw new("Invalid netplan");

        // Ethernets
        YamlMappingNode? ethernets = root.GetValue<YamlMappingNode>("ethernets");
        if (ethernets is null)
        {
            ethernets = new();
            root.SetValue("ethernets", ethernets);
        }

        // Bridges
        IEnumerable<Interface> bridges = instance.Interfaces.Where(x => x.Type == NetworkType.Bridge);
        foreach (Interface bridge in bridges)
        {
            YamlMappingNode match = new()
            {
                { "macaddress", bridge.MacAddress }
            };
            YamlMappingNode nameservers = new()
            {
                { "addresses", bridge.Dns }
            };
            YamlSequenceNode routes = new(bridge
                .Gateways
                .Select(address => new YamlMappingNode
                {
                    { "to", "default" },
                    { "via", address },
                    { "metric", "50" }
                }));
            YamlMappingNode ethernet = new()
            {
                { "dhcp4", "no" },
                // { "dhcp6", "no" },
                { "match", match },
                { "addresses", bridge.Addresses },
                { "nameservers", nameservers },
                { "routes", routes }
            };
            ethernets.SetValue(bridge.Name, ethernet);
        }

        // RoutedNic
        IEnumerable<Interface> routedNics = instance.Interfaces.Where(x => x.Type == NetworkType.RoutedNic);
        foreach (Interface routedNic in routedNics)
        {
            YamlMappingNode match = new()
            {
                { "macaddress", routedNic.MacAddress }
            };
            YamlMappingNode nameservers = new()
            {
                { "addresses", routedNic.Dns }
            };
            YamlSequenceNode routes = new(routedNic
                .Gateways
                .Select(address => new YamlMappingNode
                {
                    { "to", "default" },
                    { "via", address },
                    { "metric", "50" },
                    { "on-link", "true" }
                }));
            YamlMappingNode ethernet = new()
            {
                { "dhcp4", "no" },
                // { "dhcp6", "no" },
                { "match", match },
                { "addresses", routedNic.Addresses },
                { "nameservers", nameservers },
                { "routes", routes }
            };
            ethernets.SetValue(routedNic.Name, ethernet);
        }

        // Serialize
        string output = _serializer.Serialize(yaml);
        return output.TrimEnd('\n');
    }
}
