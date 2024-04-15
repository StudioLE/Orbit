using Orbit.Schema;

namespace Orbit.Provision;

// TODO: Replace with ClientProvider
/// <summary>
///
/// </summary>
public static class ClientProviderHelpers
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public static bool Put(this IEntityProvider<Client> provider, Client client)
    {
        return provider.Put(client.Name, client);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static IEnumerable<Client> GetAll(this IEntityProvider<Client> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new ClientId(x)))
            .OfType<Client>();
    }
}
