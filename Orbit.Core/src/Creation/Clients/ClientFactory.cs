using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using StudioLE.Patterns;

namespace Orbit.Creation.Clients;

/// <summary>
/// A factory for creating <see cref="Client"/> with default values.
/// </summary>
public class ClientFactory : IFactory<Client, Client>
{
    private const int DefaultClientNumber = 100;

    private readonly IEntityProvider<Network> _networks;
    private readonly IEntityProvider<Client> _clients;
    private readonly WireGuardClientFactory _wireGuardClientFactory;

    /// <summary>
    /// The DI constructor for <see cref="ClientFactory"/>.
    /// </summary>
    public ClientFactory(
        IEntityProvider<Network> networks,
        IEntityProvider<Client> clients,
        WireGuardClientFactory wireGuardClientFactory)
    {
        _networks = networks;
        _clients = clients;
        _wireGuardClientFactory = wireGuardClientFactory;
    }

    /// <inheritdoc/>
    public Client Create(Client source)
    {
        Client result = new();

        result.Networks = source.Networks.Any()
            ? source.Networks
            : DefaultNetworks();

        result.Number = source.Number == default
            ? DefaultNumber()
            : source.Number;

        result.Name = source.Name.IsNullOrEmpty()
            ? $"client-{result.Number:00}"
            : source.Name;

        result.WireGuard = _wireGuardClientFactory.Create(result);

        return result;
    }

    private string[] DefaultNetworks()
    {
        return _networks
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
        int finalNumber = numbers.Any()
            ? numbers.Max()
            : DefaultClientNumber - 1;
        return finalNumber + 1;
    }
}
