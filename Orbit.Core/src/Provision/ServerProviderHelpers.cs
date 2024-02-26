using Orbit.Schema;

namespace Orbit.Provision;

public static class ServerProviderHelpers
{
    public static bool Put(this IEntityProvider<Server> provider, Server server)
    {
        return provider.Put(server.Name, server);
    }

    public static IEnumerable<Server> GetAll(this IEntityProvider<Server> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new ServerId(x)))
            .OfType<Server>();
    }
}
