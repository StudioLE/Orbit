using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Servers;
using StudioLE.Orbit.Utils;
using StudioLE.Orbit.Utils.Networking;
using StudioLE.Extensions.System.Exceptions;
using StudioLE.Patterns;

namespace StudioLE.Orbit.WireGuard;

/// <summary>
/// A factory for creating <see cref="WireGuardClient"/> with default values.
/// </summary>
public class WireGuardClientFactory : IFactory<IHasWireGuardClient, Task<WireGuardClient[]>>
{
    private readonly IWireGuardFacade _wg;
    private readonly ServerProvider _servers;
    private readonly WireGuardInterfaceFactory _wgInterfaceFactory;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardClientFactory"/>.
    /// </summary>
    public WireGuardClientFactory(IWireGuardFacade wg, ServerProvider servers, WireGuardInterfaceFactory wgInterfaceFactory)
    {
        _wg = wg;
        _servers = servers;
        _wgInterfaceFactory = wgInterfaceFactory;
    }

    /// <inheritdoc/>
    public async Task<WireGuardClient[]> Create(IHasWireGuardClient entity)
    {
        return entity switch
        {
            Instance instance => await Create(instance),
            Client client => await Create(client),
            _ => throw new TypeSwitchException<IHasWireGuardClient>(string.Empty, entity)
        };
    }

    private ValueTask<WireGuardClient[]> Create<T>(T entity) where T : struct, IHasWireGuardClient
    {
        if (entity.WireGuard.IsDefault())
            entity.WireGuard = entity
                .Connections
                .Select(server => new WireGuardClient
                {
                    Server = server
                })
                .ToArray();
        return entity
            .WireGuard
            .ToAsyncEnumerable()
            .SelectAwait(async wg => await Create(wg, entity))
            .ToArrayAsync();
    }

    private async Task<WireGuardClient> Create(WireGuardClient wg, IHasWireGuardClient entity)
    {
        if (wg.Server.IsDefault())
            throw new($"{nameof(WireGuardClient.Server)} must be set.");
        Server server = await _servers.Get(wg.Server) ?? throw new("Failed to get the server.");
        if(wg.Name.IsDefault())
            wg.Name = _wgInterfaceFactory.GetName(server);
        if (wg.Port.IsDefault())
            wg.Port = _wgInterfaceFactory.GetPort(entity, server);
        if (wg.Addresses.IsDefault())
            wg.Addresses =
            [
                _wgInterfaceFactory.GetIPv4Address(entity, server),
                _wgInterfaceFactory.GetIPv6Address(entity, server)
            ];
        if (wg.PrivateKey.IsDefault())
            wg.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");
        if (wg.PublicKey.IsDefault())
            wg.PublicKey = _wg.GeneratePublicKey(wg.PrivateKey) ?? throw new("Failed to generate WireGuard public key");
        if (wg.PreSharedKey.IsDefault())
            wg.PreSharedKey = _wg.GeneratePreSharedKey() ?? throw new("Failed to generate WireGuard pre-shared key");
        if (wg.AllowedIPs.IsDefault())
            wg.AllowedIPs =
            [
                _wgInterfaceFactory.GetIPv4Subnet(server),
                _wgInterfaceFactory.GetIPv6Subnet(server)
            ];
        if (wg.Endpoint.IsDefault())
            wg.Endpoint = GetEndpoint(entity, server);
        return wg;
    }

    private static string GetEndpoint(IHasWireGuardClient entity, Server server)
    {
        bool isExternal = entity is not Instance instance
                          || instance.Server != server.Name;
        Interface iface = isExternal
            ? GetNicInterface(server)
            : GetBridgeInterface(server);
        string endpoint = iface
                              .Addresses
                              .FirstOrDefault()
                          ?? throw new($"Failed to get the endpoint for {entity.GetType()}.");
        endpoint = IPHelpers.RemoveCidr(endpoint);
        endpoint += ":" + server.WireGuard.Port;
        return endpoint;
    }

    private static Interface GetBridgeInterface(Server server)
    {
        Interface? interfaceQuery = server
            .Interfaces
            .FirstOrNull(x => x.Type == NetworkType.Bridge && x.Server == server.Name);
        if (interfaceQuery is not Interface iface)
            throw new($"Bridge not found for server: {server.Name}.");
        return iface;
    }

    private static Interface GetNicInterface(Server server)
    {
        Interface? interfaceQuery = server
            .Interfaces
            .FirstOrNull(x => x.Type == NetworkType.Nic);
        if (interfaceQuery is not Interface iface)
            throw new($"NIC not found for server: {server.Name}.");
        return iface;
    }

    private static MacAddress GetMacAddress(IEntity entity, Server server)
    {
        return MacAddressHelpers.Generate((int)NetworkType.WireGuard, server.Number, entity.Number);
    }
}
