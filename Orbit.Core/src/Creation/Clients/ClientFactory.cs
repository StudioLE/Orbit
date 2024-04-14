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
            client.Name = new($"client-{client.Number:00}");
        if (client.Interfaces.IsDefault())
            client.Interfaces = DefaultInterfaces(client);
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

    private static Interface[] DefaultInterfaces(Client client)
    {
        return
        [
            new()
            {
                Name = "eth0",
                Type = NetworkType.Nic,
                MacAddress = GetMacAddress(client),
                Addresses = ["203.0.113.1", "2001:db8::"]
            }
        ];
    }

    private static string GetMacAddress(Client client)
    {
        return MacAddressHelpers.Generate(2, 0, client.Number);
    }
}
