using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
using Orbit.WireGuard;
using StudioLE.Patterns;

namespace Orbit.Creation.Clients;

/// <summary>
/// A factory for creating <see cref="Client"/> with default values.
/// </summary>
public class ClientFactory : IFactory<Client, Client>
{
    private const int DefaultClientNumber = 100;

    private readonly IEntityProvider<Server> _servers;
    private readonly IEntityProvider<Client> _clients;
    private readonly WireGuardClientFactory _wireGuardClientFactory;

    /// <summary>
    /// The DI constructor for <see cref="ClientFactory"/>.
    /// </summary>
    public ClientFactory(
        IEntityProvider<Server> servers,
        IEntityProvider<Client> clients,
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
            client.Name = $"client-{client.Number:00}";
        if (client.Interfaces.IsDefault())
            client.Interfaces = DefaultInterfaces();
        if (client.WireGuard.IsDefault())
            client.WireGuard = _wireGuardClientFactory.Create(client);
        return client;
    }

    private string[] DefaultServers()
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

    private static Interface[] DefaultInterfaces()
    {
        return
        [
            new()
            {
                Name = "eth0",
                Type = NetworkType.Nic,
                MacAddress = MacAddressHelpers.Generate(),
                Addresses = ["203.0.113.1", "2001:db8::"]
            }
        ];
    }
}
