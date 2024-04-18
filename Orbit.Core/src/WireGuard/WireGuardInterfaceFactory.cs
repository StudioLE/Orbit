using Orbit.Schema;
using Orbit.Utils.Networking;

namespace Orbit.WireGuard;

/// <summary>
/// Create a bridge interface for an <see cref="IEntity"/>.
/// </summary>
public class WireGuardInterfaceFactory
{
    /// <summary>
    /// Get the interface name
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>The interface name.</returns>
    public string GetName(Server server)
    {
        return "wg" + server.Number;
    }

    /// <summary>
    /// Get the IPv4 DNS address for the entity.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 DNS address for the entity.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv4Dns(Server server)
    {
        return $"10.1.{server.Number}.2";
    }

    /// <summary>
    /// Get the IPv6 DNS address for the entity.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 DNS address for the entity.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv6Dns(Server server)
    {
        return $"fc00::1:{server.Number}:2";
    }


    /// <summary>
    /// Get the IPv4 address for the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 address for the entity.
    /// </returns>
    public string GetIPv4Address(IEntity entity, Server server)
    {
        return $"10.1.{server.Number}.{entity.Number}";
    }


    /// <summary>
    /// Get the IPv4 address with CIDR for the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 address with CIDR for the entity.
    /// </returns>
    public string GetIPv4AddressWithCidr(IEntity entity, Server server)
    {
        return GetIPv4Address(entity, server) + "/24";
    }

    /// <summary>
    /// Get the IPv6 address for the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 address for the entity.
    /// </returns>
    public string GetIPv6Address(IEntity entity, Server server)
    {
        return $"fc00::1:{server.Number}:{entity.Number}";
    }


    /// <summary>
    /// Get the IPv6 address with CIDR for the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 address with CIDR for the entity.
    /// </returns>
    public string GetIPv6AddressWithCidr(IEntity entity, Server server)
    {
        return GetIPv6Address(entity, server) + "/112";
    }

    /// <summary>
    /// Get the IPv4 gateway address for the entity.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 gateway address for the entity.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv4Gateway(Server server)
    {
        return $"10.1.{server.Number}.1";
    }

    /// <summary>
    /// Get the IPv6 gateway address for the entity.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 gateway address for the entity.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv6Gateway(Server server)
    {
        return $"fc00::1:{server.Number}:1";
    }

    /// <summary>
    /// Get the IPv4 subnet for the entity.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 subnet for the entity.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv4Subnet(Server server)
    {
        return $"10.1.{server.Number}.0/24";
    }

    /// <summary>
    /// Get the IPv6 subnet for the entity.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 subnet for the entity.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv6Subnet(Server server)
    {
        return $"fc00::1:{server.Number}:0/112";
    }

    /// <summary>
    /// Get the deterministic MAC address for the entity.
    /// </summary>
    /// <remarks>
    /// The MAC address is deterministic based on the server number and entity number.
    /// </remarks>
    /// <param name="entity">The entity.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The MAC address for the entity.
    /// </returns>
    public MacAddress GetMacAddress(IEntity entity, Server server)
    {
        return MacAddressHelpers.Generate((int)NetworkType.WireGuard, server.Number, entity.Number);
    }
}
