using Orbit.Schema;
using Orbit.Utils.Networking;
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
            string range = "default";
            if (index != 0 && IPv4Parser.Parse(address) is IPv4 ipv4)
            {
                // TODO: Range should be determined by inspecting CIDR of interface
                byte[] octets = ipv4.Octets
                    .SkipLast(1)
                    .Append((byte)0)
                    .ToArray();
                IPv4 ipv4Range = new(octets, 24);
                range = ipv4Range.ToString();
            }
            else if (index != 0 && IPv6Parser.Parse(address) is IPv6 ipv6)
            {
                // TODO: Range should be determined by inspecting CIDR of interface
                ushort[] hextets = ipv6.GetHextets()
                    .SkipLast(1)
                    .Append((ushort)0)
                    .ToArray();
                IPv6 ipv6Range = new(hextets, 112);
                range = $"'{ipv6Range}'";
            }
            output += $"""

                      - to: {range}
                        via: {address.AsYamlString()}
                        metric: 50
                """;
            if (iface.Type == NetworkType.RoutedNic)
                output += "\non-link: true".Indent(4);
        }
        return output;
    }
}
