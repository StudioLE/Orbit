using Orbit.Core.Providers;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;

namespace Orbit.Core.Schema;

public sealed class Network
{
    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string Address { get; set; } = string.Empty;

    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string Gateway { get; set; } = string.Empty;

    public void Review(Instance instance, EntityProvider provider)
    {
        Server server = provider.Server.Get(instance.Server) ?? throw new("Failed to get host.");
        Cluster cluster = provider.Cluster.Get(instance.Cluster) ?? throw new("Failed to get cluster.");
        if (Address.IsNullOrEmpty())
            Address = $"10.{server.Number}.{cluster.Number}.{instance.Number}";
        if (Gateway.IsNullOrEmpty())
            Gateway = $"10.{server.Number}.0.1";
    }
}
