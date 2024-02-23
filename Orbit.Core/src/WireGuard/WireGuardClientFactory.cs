using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
using StudioLE.Extensions.System.Exceptions;
using StudioLE.Patterns;

namespace Orbit.WireGuard;

/// <summary>
/// A factory for creating <see cref="WireGuardClient"/> with default values.
/// </summary>
public class WireGuardClientFactory : IFactory<IHasWireGuardClient, WireGuardClient[]>
{
    private readonly IWireGuardFacade _wg;
    private readonly IEntityProvider<Server> _servers;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardClientFactory"/>.
    /// </summary>
    public WireGuardClientFactory(IWireGuardFacade wg, IEntityProvider<Server> servers)
    {
        _wg = wg;
        _servers = servers;
    }

    /// <inheritdoc/>
    public WireGuardClient[] Create(IHasWireGuardClient entity)
    {
        return entity switch
        {
            Instance instance => Create(instance),
            Client client => Create(client),
            _ => throw new TypeSwitchException<IHasWireGuardClient>(string.Empty, entity)
        };
    }

    private WireGuardClient[] Create<T>(T entity) where T : struct, IHasWireGuardClient
    {
        if (entity.WireGuard.IsDefault())
            entity = entity with
            {
                WireGuard = entity
                    .Connections
                    .Select(network => new WireGuardClient
                    {
                        Interface = new()
                        {
                            Server = network
                        }
                    })
                    .ToArray()
            };
        return entity
            .WireGuard
            .Select(wg => Create(wg, entity))
            .ToArray();
    }

    private WireGuardClient Create(WireGuardClient wg, IHasWireGuardClient entity)
    {
        if (wg.Interface.Server.IsDefault())
            throw new($"{nameof(WireGuardClient.Interface.Server)} must be set.");
        Server server = _servers.Get(new ServerId(wg.Interface.Server)) ?? throw new("Failed to get the server.");
        wg = wg with
        {
            Interface = ApplyInterfaceDefaults(wg.Interface, entity, server)
        };
        if (wg.Port.IsDefault())
            wg = wg with
            {
                Port = server.WireGuard.Port
            };
        if(wg.PrivateKey.IsDefault())
            wg = wg with
            {
                PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key")
            };
        if(wg.PublicKey.IsDefault())
            wg = wg with
            {
                PublicKey = _wg.GeneratePublicKey(wg.PrivateKey) ?? throw new("Failed to generate WireGuard public key")
            };
        if(wg.PreSharedKey.IsDefault())
            wg = wg with
            {
                PreSharedKey = _wg.GeneratePreSharedKey() ?? throw new("Failed to generate WireGuard pre-shared key")
            };
        if(wg.AllowedIPs.IsDefault())
            wg = wg with
            {
                AllowedIPs = wg.Interface.Subnets
            };
        if(wg.Endpoint.IsDefault())
            wg = wg with
            {
                Endpoint = GetEndpoint(entity, server)
            };
        return wg;
    }

    private static Interface ApplyInterfaceDefaults(Interface iface, IEntity entity, Server server)
    {
        if(iface.Name.IsDefault())
            iface = iface with
            {
                Name = $"wg{server.Number}"
            };
        if(iface.Type.IsDefault())
            iface = iface with
            {
                Type = NetworkType.WireGuard
            };
        if(iface.MacAddress.IsDefault())
            iface = iface with
            {
                MacAddress = MacAddressHelpers.Generate()
            };
        if(iface.Addresses.IsDefault())
            iface = iface with
            {
                Addresses =
                [
                    $"10.1.{server.Number}.{entity.Number}/24",
                    $"fc00::1:{server.Number}:{entity.Number}/112"
                ]
            };
        if(iface.Gateways.IsDefault())
            iface = iface with
            {
                Gateways =
                [
                    $"10.1.{server.Number}.1",
                    $"fc00::1:{server.Number}:1"
                ]
            };
        if(iface.Subnets.IsDefault())
            iface = iface with
            {
                Subnets =
                [
                    $"10.1.{server.Number}.0/24",
                    $"fc00::1:{server.Number}:0/112"
                ]
            };
        if(iface.Dns.IsDefault())
            iface = iface with
            {
                Dns =
                [
                    $"10.1.{server.Number}.2",
                    $"fc00::1:{server.Number}:2"
                ]
            };
        return iface;
    }

    private string GetEndpoint(IHasWireGuardClient entity, Server server)
    {
        bool isExternal = entity is not Instance instance
                          || instance.Server != server.Name;
        Interface iface = isExternal
            ? server
                  .Interfaces
                  .FirstOrNull(x => x.Type == NetworkType.Nic)
              ?? throw new($"NIC not found for server: {server.Name}.")
            : server
                  .Interfaces
                  .FirstOrNull(x => x.Type == NetworkType.Bridge && x.Server == server.Name)
              ?? throw new($"Bridge not found for server: {server.Name}.");
        string endpoint = iface
                .Addresses
                .FirstOrDefault()
            ?? throw new($"Failed to get the endpoint for {entity.GetType()}.");
        endpoint = IPHelpers.RemoveCidr(endpoint);
        endpoint += ":" + server.WireGuard.Port;
        return endpoint;
    }
}
