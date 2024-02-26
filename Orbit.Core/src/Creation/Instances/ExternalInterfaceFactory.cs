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
        string? ipv6 = nic.Addresses.FirstOrDefault(x => x.EndsWith("::"));
        if (ipv6 is null)
            return null;
        return new()
        {
            Name = "ext" + server.Number,
            Server = server.Name,
            Type = NetworkType.RoutedNic,
            MacAddress = MacAddressHelpers.Generate(),
            Addresses =
            [
                ipv6
            ],
            // TODO: Add subnet
            Gateways = nic.Gateways,
            Dns = nic.Dns
        };
    }
}
