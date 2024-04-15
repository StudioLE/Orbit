using Orbit.Schema;
using Orbit.Servers;
using Orbit.Utils;
using Orbit.WireGuard;
using StudioLE.Patterns;

namespace Orbit.Clients;

/// <summary>
/// A factory for creating <see cref="Client"/> with default values.
/// </summary>
public class ClientFactory : IFactory<Client, Client>
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
    public Client Create(Client client)
    {
        if (client.Connections.IsDefault())
            client.Connections = DefaultServers();
        if (client.Number.IsDefault())
            client.Number = DefaultNumber();
        if (client.Name.IsDefault())
            client.Name = new($"client-{client.Number:00}");
        if (client.WireGuard.IsDefault())
            client.WireGuard = _wireGuardClientFactory.Create(client);
        return client;
    }

    private ServerId[] DefaultServers()
    {
        return _servers
            .GetAll()
            .Select(x => x.Name)
            .ToArray();
    }

    private int DefaultNumber()
    {
        int[] numbers = _clients
            .GetAll()
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Length != 0
            ? numbers.Max()
            : DefaultClientNumber - 1;
        return finalNumber + 1;
    }
}
