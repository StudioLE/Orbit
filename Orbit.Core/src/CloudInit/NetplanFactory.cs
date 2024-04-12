using Orbit.Schema;
using Orbit.Utils.Yaml;
using StudioLE.Extensions.System;
using StudioLE.Patterns;
using StudioLE.Serialization;

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
        string interfaces = instance
            .Interfaces
            .Select(SerializeInterface)
            .Join();
        return $"""
            network:
              version: 2
              ethernets:
            {interfaces}

            """;
    }

    private static string SerializeInterface(Interface iface, int index)
    {
        string output = $"""
                {iface.Name}:
                  dhcp4: no
                  match:
                    macaddress: {iface.MacAddress}
                  addresses:{iface.Addresses.AsYamlSequence(3)}
                  nameservers:
                    addresses:{iface.Dns.AsYamlSequence(4)}
                  routes:
            """;
        foreach (string address in iface.Gateways)
        {
            output += $"""

                      - to: default
                        via: {address.AsYamlString()}
                        metric: {(index + 1) * 10}
                """;
            if (iface.Type == NetworkType.RoutedNic)
                output += "\non-link: true".Indent(4);
        }
        return output;
    }
}
