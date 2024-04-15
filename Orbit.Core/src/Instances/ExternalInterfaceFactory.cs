using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;

namespace Orbit.Instances;

/// <summary>
/// A factory to create a routed nic <see cref="Interface"/> for an <see cref="Instance"/>.
/// </summary>
public class ExternalInterfaceFactory
{
    public string GetName(Server server)
    {
        return "ext" + server.Number;
    }

    public IPv6? GetIPv6Address(Instance instance, Interface nic)
    {
        IPv6? ipv6Query = nic
            .Subnets
            .Select(IPv6Parser.Parse)
            .OfType<IPv6>()
            .FirstOrDefault();
        if (ipv6Query is not IPv6 ipv6)
            return null;
        ushort[] hextets = ipv6.GetHextets();
        if(hextets.Length != 8)
            throw new("Expected 8 hextets in IPv6 address.");
        hextets[^1] = HexadecimalHelpers.ToUShort(instance.Number.ToString()) ?? throw new("Invalid instance number.");
        return new(hextets);
    }

    public string GetIPv6AddressWithCidr(Instance instance, Interface nic)
    {
        return GetIPv6Address(instance, nic) + "/128";
    }

    public string GetMacAddress(Instance instance, Server server)
    {
        return MacAddressHelpers.Generate(3, server.Number, instance.Number);
    }
}
