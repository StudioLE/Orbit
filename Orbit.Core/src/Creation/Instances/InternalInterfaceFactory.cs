using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.Networking;
using StudioLE.Patterns;

namespace Orbit.Creation.Instances;

/// <summary>
/// A factory for creating a network <see cref="Interface"/> to connect to the internal network.
/// </summary>
public class InternalInterfaceFactory : IFactory<Instance, Interface>
{
    private readonly IEntityProvider<Server> _servers;

    public InternalInterfaceFactory(IEntityProvider<Server> servers)
    {
        _servers = servers;
    }

    /// <inheritdoc/>
    public Interface Create(Instance instance)
    {
        Server server = _servers.Get(instance.Server) ?? throw new($"Server not found: {instance.Server}.");
        return new()
        {
            Name = "int" + server.Number,
            Server = server.Name,
            Type = NetworkType.Bridge,
            MacAddress = GetMacAddress(instance, server),
            Addresses =
            [
                $"10.0.{server.Number}.{instance.Number}/24",
                $"fc00::0:{server.Number}:{instance.Number}/112"
            ],
            Gateways = [
                $"10.0.{server.Number}.1",
                $"fc00::0:{server.Number}:1"
            ],
            // TODO: Set the subnets
            Subnets = Array.Empty<string>(),
            Dns = [
                $"10.0.{server.Number}.2",
                $"fc00::0:{server.Number}:2"
            ]
        };
    }

    private static string GetMacAddress(Instance instance, Server server)
    {
        return MacAddressHelpers.Generate(1, server.Number, instance.Number);
    }
}
