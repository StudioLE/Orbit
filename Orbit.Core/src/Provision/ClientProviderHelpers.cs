using Orbit.Schema;

namespace Orbit.Provision;

public static class ClientProviderHelpers
{
    public static bool Put(this IEntityProvider<Client> provider, Client client)
    {
        return provider.Put(client.Name, client);
    }

    public static IEnumerable<Client> GetAll(this IEntityProvider<Client> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new ClientId(x)))
            .OfType<Client>();
    }
}
