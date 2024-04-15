using Orbit.Schema;

namespace Orbit.Provision;

// TODO: Replace with ServerProvider
/// <summary>
///
/// </summary>
public static class ServerProviderHelpers
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="server"></param>
    /// <returns></returns>
    public static bool Put(this IEntityProvider<Server> provider, Server server)
    {
        return provider.Put(server.Name, server);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static IEnumerable<Server> GetAll(this IEntityProvider<Server> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new ServerId(x)))
            .OfType<Server>();
    }
}
