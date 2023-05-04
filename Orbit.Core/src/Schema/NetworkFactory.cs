using Orbit.Core.Providers;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

public class NetworkFactory : IFactory<Instance, Network>
{
    private readonly EntityProvider _provider;

    public NetworkFactory(EntityProvider provider)
    {
        _provider = provider;
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

        Server server = _provider.Server.Get(instance.Server) ?? throw new("Failed to get host.");
        Cluster cluster = _provider.Cluster.Get(instance.Cluster) ?? throw new("Failed to get cluster.");

        if (result.Address.IsNullOrEmpty())
            result.Address = $"10.{server.Number}.{cluster.Number}.{instance.Number}";

        if (result.Gateway.IsNullOrEmpty())
            result.Gateway = $"10.{server.Number}.0.1";

        return result;
    }
}
