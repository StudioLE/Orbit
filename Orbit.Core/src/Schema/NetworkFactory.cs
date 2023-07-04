using Orbit.Core.Provision;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

/// <summary>
/// A factory for creating <see cref="Network"/> with default values.
/// </summary>
public class NetworkFactory : IFactory<Instance, Network>
{
    private readonly IEntityProvider<Server> _servers;


    public NetworkFactory(IEntityProvider<Server> servers)
    {
        _servers = servers;
    }

    /// <inheritdoc />
    public Network Create(Instance instance)
    {
        Network result = new()
        {
            Address = instance.Network.Address,
            Gateway = instance.Network.Gateway
        };

        if(!result.Address.IsNullOrEmpty() && !result.Gateway.IsNullOrEmpty())
            return result;

        Server server = _servers.Get(new ServerId(instance.Server)) ?? throw new("Failed to get server.");

        if (result.Address.IsNullOrEmpty())
            result.Address = $"10.{server.Number}.{instance.Number}.0";

        if (result.Gateway.IsNullOrEmpty())
            result.Gateway = $"10.{server.Number}.0.1";

        return result;
    }
}
