using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
using StudioLE.Patterns;

namespace Orbit.Creation.Instances;

/// <summary>
/// A factory for creating a network <see cref="Interface"/> to connect to the external network.
/// </summary>
public class ExternalInterfaceFactory : IFactory<Instance, Interface?>
{
    private readonly IEntityProvider<Server> _servers;

    public ExternalInterfaceFactory(IEntityProvider<Server> servers)
    {
        _servers = servers;
    }

    /// <inheritdoc/>
    public Interface? Create(Instance instance)
    {
        Server server = _servers.Get(instance.Server) ?? throw new($"Server not found: {instance.Server}.");
        Interface nic = server
                            .Interfaces
                            .FirstOrNull(x => x.Type == NetworkType.Nic)
                        ?? throw new($"NIC not found for server: {instance.Server}.");
        IPv6? ipv6Query = GetIPv6(nic, instance);
        if (ipv6Query is not IPv6 ipv6)
            return null;
        return new()
        {
            Name = "ext" + server.Number,
            Server = server.Name,
            Type = NetworkType.RoutedNic,
            MacAddress = MacAddressHelpers.Generate(),
            Addresses =
            [
                ipv6.ToString()
            ],
            // TODO: Add subnet
            Gateways = nic.Gateways.Where(IPHelpers.IsIPv6).ToArray(),
            Dns = nic.Dns
        };
    }

    private static IPv6? GetIPv6(Interface nic, Instance instance)
    {
        string? ipv6Str = nic.Subnets.FirstOrDefault(IPHelpers.IsIPv6);
        if (ipv6Str is null)
            return null;
        ipv6Str = IPHelpers.RemoveCidr(ipv6Str);
        IPv6? ipv6Query = IPv6Parser.Parse(ipv6Str);
        if (ipv6Query is not IPv6 ipv6)
            return null;
        ushort[] hextets = ipv6.GetHextets();
        int count = 8 - hextets.Length;
        if (count > 0)
        {
            ushort[] padding = Enumerable.Repeat((ushort)0, count).ToArray();
            hextets = hextets.Concat(padding).ToArray();
        }

        hextets[^1] = HexadecimalHelpers.ToUShort(instance.Number.ToString()) ?? throw new("Invalid instance number.");
        return new(hextets, 128);
    }
}
