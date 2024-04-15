using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;

namespace Orbit.Instances;

/// <summary>
/// Create a routed nic interface for an <see cref="Instance"/>.
/// </summary>
public class ExternalInterfaceFactory
{
    /// <summary>
    /// Get the interface name
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>The interface name.</returns>
    public string GetName(Server server)
    {
        return "ext" + server.Number;
    }

    /// <summary>
    /// Get the IPv6 address for the instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="nic">The nic interface of the server.</param>
    /// <returns>
    /// The IPv6 address for the instance.
    /// </returns>
    public IPv6? GetIPv6Address(Instance instance, Interface nic)
    {
        IPv6? ipv6Query = nic
            .Subnets
            .Select(IPv6Parser.Parse)
            .OfType<IPv6>()
            .FirstOrDefault();
        if (ipv6Query is not IPv6 ipv6)
            return null;
        ushort[] hextets = ipv6.Hextets;
        if(hextets.Length != 8)
            throw new("Expected 8 hextets in IPv6 address.");
        hextets[^1] = HexadecimalHelpers.ToUShort(instance.Number.ToString()) ?? throw new("Invalid instance number.");
        return new(hextets);
    }

    /// <summary>
    /// Get the IPv6 address with CIDR for the instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="nic">The nic interface of the server.</param>
    /// <returns>
    /// The IPv6 address for the instance with CIDR.
    /// </returns>
    public string GetIPv6AddressWithCidr(Instance instance, Interface nic)
    {
        return GetIPv6Address(instance, nic) + "/128";
    }

    /// <summary>
    /// Get the deterministic MAC address for the instance.
    /// </summary>
    /// <remarks>
    /// The MAC address is deterministic based on the server number and instance number.
    /// </remarks>
    /// <param name="instance">The instance.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The MAC address for the instance.
    /// </returns>
    public MacAddress GetMacAddress(Instance instance, Server server)
    {
        return MacAddressHelpers.Generate(3, server.Number, instance.Number);
    }
}
