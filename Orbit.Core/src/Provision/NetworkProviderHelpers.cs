using Orbit.Schema;

namespace Orbit.Provision;

public static class NetworkProviderHelpers
{
    public static bool Put(this IEntityProvider<Network> provider, Network server)
    {
        return provider.Put(new NetworkId(server.Name), server);
    }

    public static IEnumerable<Network> GetAll(this IEntityProvider<Network> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new NetworkId(x)))
            .OfType<Network>();
    }
}
