using Orbit.Schema;
using Orbit.Utils.Networking;

namespace Orbit.Creation.Instances;

/// <summary>
/// A factory for creating a network <see cref="Interface"/> to connect to the internal network.
/// </summary>
public class InternalInterfaceFactory
{
    public string GetIPv6Gateway(Server server)
    {
        return $"fc00::0:{server.Number}:1";
    }

    public string GetIPv4Gateway(Server server)
    {
        return $"10.0.{server.Number}.1";
    }

    public string GetName(Server server)
    {
        return "int" + server.Number;
    }

    public string GetIPv6Dns(Server server)
    {
        return $"fc00::0:{server.Number}:2";
    }

    public string GetIPv4Dns(Server server)
    {
        return $"10.0.{server.Number}.2";
    }

    public string GetIPv6Address(Instance instance, Server server)
    {
        return $"fc00::0:{server.Number}:{instance.Number}";
    }

    public string GetIPv6AddressWithCidr(Instance instance, Server server)
    {
        return GetIPv6Address(instance, server) + "/112";
    }

    public string GetIPv4Address(Instance instance, Server server)
    {
        return $"10.0.{server.Number}.{instance.Number}";
    }

    public string GetIPv4AddressWithCidr(Instance instance, Server server)
    {
        return GetIPv4Address(instance, server) + "/24";
    }

    public string GetMacAddress(Instance instance, Server server)
    {
        return MacAddressHelpers.Generate(1, server.Number, instance.Number);
    }
}
