using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
using Orbit.WireGuard;
using StudioLE.Patterns;

namespace Orbit.Servers;

/// <summary>
/// A factory for creating <see cref="Server"/> with default values.
/// </summary>
public class ServerFactory : IFactory<Server, Task<Server>>
{
    private const string DefaultName = "server";
    private const int DefaultNumberValue = 1;
    private readonly ServerProvider _servers;
    private readonly BridgeInterfaceFactory _bridgeFactory;
    private readonly WireGuardServerFactory _wireGuardServerFactory;

    /// <summary>
    /// DI constructor for <see cref="ServerFactory"/>.
    /// </summary>
    public ServerFactory(
        ServerProvider servers,
        BridgeInterfaceFactory bridgeFactory,
        WireGuardServerFactory wireGuardServerFactory)
    {
        _servers = servers;
        _bridgeFactory = bridgeFactory;
        _wireGuardServerFactory = wireGuardServerFactory;
    }

    /// <inheritdoc />
    public async Task<Server> Create(Server server)
    {
        if (server.Number.IsDefault())
            server.Number = await DefaultNumber();
        if (server.Name.IsDefault())
            server.Name = new($"{DefaultName}-{server.Number:00}");
        if (server.Interfaces.IsDefault())
            server.Interfaces = DefaultInterfaces(server);
        server.WireGuard = _wireGuardServerFactory.Create(server);
        return server;
    }

    private Interface[] DefaultInterfaces(Server server)
    {
        Interface nic = new()
        {
            Name = "eth0",
            Server = server.Name,
            Type = NetworkType.Nic,
            MacAddress = GetMacAddress(server),
            Addresses = ["203.0.113.64/24", "2001:db8::64/112"],
            Gateways = ["203.0.113.255", "2001:db8::"],
            Subnets = ["203.0.113.0/24", "2001:db8::/112"],
            Dns = ["1.1.1.1", "1.0.0.1", "2606:4700:4700::1111", "2606:4700:4700::1001"]
        };
        Interface bridge = _bridgeFactory.Create(server);
        return [nic, bridge];
    }

    private static MacAddress GetMacAddress(Server server)
    {
        return MacAddressHelpers.Generate((int)NetworkType.RoutedNic + 5, server.Number, 0);
    }

    private async Task<int> DefaultNumber()
    {
        Server[] servers = await _servers.GetAll();
        int[] numbers = servers
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Length != 0
            ? numbers.Max()
            : DefaultNumberValue - 1;
        return finalNumber + 1;
    }
}
