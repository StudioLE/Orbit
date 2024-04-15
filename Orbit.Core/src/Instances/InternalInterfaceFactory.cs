using Orbit.Schema;
using Orbit.Utils.Networking;

namespace Orbit.Instances;

/// <summary>
/// Create a bridge interface for an <see cref="Instance"/>.
/// </summary>
public class InternalInterfaceFactory
{
    /// <summary>
    /// Get the interface name
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>The interface name.</returns>
    public string GetName(Server server)
    {
        return "int" + server.Number;
    }

    /// <summary>
    /// Get the IPv4 DNS address for the instance.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 DNS address for the instance.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv4Dns(Server server)
    {
        return $"10.0.{server.Number}.2";
    }

    /// <summary>
    /// Get the IPv6 DNS address for the instance.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 DNS address for the instance.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv6Dns(Server server)
    {
        return $"fc00::0:{server.Number}:2";
    }


    /// <summary>
    /// Get the IPv4 address for the instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 address for the instance.
    /// </returns>
    public string GetIPv4Address(Instance instance, Server server)
    {
        return $"10.0.{server.Number}.{instance.Number}";
    }


    /// <summary>
    /// Get the IPv4 address with CIDR for the instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 address with CIDR for the instance.
    /// </returns>
    public string GetIPv4AddressWithCidr(Instance instance, Server server)
    {
        return GetIPv4Address(instance, server) + "/24";
    }

    /// <summary>
    /// Get the IPv6 address for the instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 address for the instance.
    /// </returns>
    public string GetIPv6Address(Instance instance, Server server)
    {
        return $"fc00::0:{server.Number}:{instance.Number}";
    }


    /// <summary>
    /// Get the IPv6 address with CIDR for the instance.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 address with CIDR for the instance.
    /// </returns>
    public string GetIPv6AddressWithCidr(Instance instance, Server server)
    {
        return GetIPv6Address(instance, server) + "/112";
    }

    /// <summary>
    /// Get the IPv4 gateway address for the instance.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv4 gateway address for the instance.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv4Gateway(Server server)
    {
        return $"10.0.{server.Number}.1";
    }

    /// <summary>
    /// Get the IPv6 address for the instance.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// The IPv6 address for the instance.
    /// </returns>
    //  TODO: This should be obtained from the server.
    public string GetIPv6Gateway(Server server)
    {
        return $"fc00::0:{server.Number}:1";
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
    public string GetMacAddress(Instance instance, Server server)
    {
        return MacAddressHelpers.Generate(1, server.Number, instance.Number);
    }
}
