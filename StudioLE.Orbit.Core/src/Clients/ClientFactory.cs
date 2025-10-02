using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Servers;
using StudioLE.Orbit.Utils;
using StudioLE.Orbit.WireGuard;
using StudioLE.Patterns;

namespace StudioLE.Orbit.Clients;

/// <summary>
/// A factory for creating <see cref="Client"/> with default values.
/// </summary>
public class ClientFactory : IFactory<Client, Task<Client>>
{
    private const int DefaultClientNumber = 100;

    private readonly ServerProvider _servers;
    private readonly ClientProvider _clients;
    private readonly WireGuardClientFactory _wireGuardClientFactory;

    /// <summary>
    /// The DI constructor for <see cref="ClientFactory"/>.
    /// </summary>
    public ClientFactory(
        ServerProvider servers,
        ClientProvider clients,
        WireGuardClientFactory wireGuardClientFactory)
    {
        _servers = servers;
        _clients = clients;
        _wireGuardClientFactory = wireGuardClientFactory;
    }

    /// <inheritdoc/>
    public async Task<Client> Create(Client client)
    {
        if (client.Connections.IsDefault())
            client.Connections = await DefaultServers();
        if (client.Number.IsDefault())
            client.Number = await DefaultNumber();
        if (client.Name.IsDefault())
            client.Name = new($"client-{client.Number:00}");
        if (client.WireGuard.IsDefault())
            client.WireGuard = await _wireGuardClientFactory.Create(client);
        return client;
    }

    private async Task<ServerId[]> DefaultServers()
    {
        Server[] servers = await _servers.GetAll();
        return servers
            .Select(x => x.Name)
            .ToArray();
    }

    private async Task<int> DefaultNumber()
    {
        IAsyncEnumerable<Client> clients = await _clients.GetAll();
        int[] numbers = await clients
            .Select(x => x.Number)
            .ToArrayAsync();
        int finalNumber = numbers.Length != 0
            ? numbers.Max()
            : DefaultClientNumber - 1;
        return finalNumber + 1;
    }
}
