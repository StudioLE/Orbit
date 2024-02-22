using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.Utils.Networking;
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
    public WireGuardClient[] Create(IHasWireGuardClient instance)
    {
        if (!instance.WireGuard.Any())
            instance.WireGuard = instance
                .Connections
                .Select(network => new WireGuardClient
                {
                    Interface = new()
                    {
                        Server = network
                    }
                })
                .ToArray();
        return instance
            .WireGuard
            .Select(wg => Create(wg, instance))
            .ToArray();
    }

    private WireGuardClient Create(WireGuardClient wg, IHasWireGuardClient entity)
    {
        WireGuardClient result = new()
        {
            Interface = new()
            {
                Name = wg.Interface.Name,
                Server = wg.Interface.Server,
                Type = wg.Interface.Type,
                MacAddress = wg.Interface.MacAddress,
                Addresses = wg.Interface.Addresses,
                Gateways = wg.Interface.Gateways,
                Subnets = wg.Interface.Subnets,
                Dns = wg.Interface.Dns
            },
            Port = wg.Port,
            PrivateKey = wg.PrivateKey,
            PublicKey = wg.PublicKey,
            PreSharedKey = wg.PreSharedKey,
            AllowedIPs = wg.AllowedIPs,
            Endpoint = wg.Endpoint
        };

        if (result.Interface.Server.IsNullOrEmpty())
            throw new("Network must be set.");

        Server server = _servers.Get(new ServerId(wg.Interface.Server)) ?? throw new("Failed to get the server.");

        if (result.Interface.Name.IsNullOrEmpty())
            result.Interface.Name = $"wg{server.Number}";

        if (result.Interface.Type == default)
            result.Interface.Type = NetworkType.WireGuard;

        if (result.Interface.MacAddress.IsNullOrEmpty())
            result.Interface.MacAddress = MacAddressHelpers.Generate();

        if(!result.Interface.Addresses.Any())
            result.Interface.Addresses =
            [
                $"10.1.{server.Number}.{entity.Number}/24",
                $"fc00::1:{server.Number}:{entity.Number}/112"
            ];

        if(!result.Interface.Gateways.Any())
            result.Interface.Gateways =
            [
                $"10.1.{server.Number}.1",
                $"fc00::1:{server.Number}:1"
            ];

        if(!result.Interface.Subnets.Any())
            result.Interface.Subnets =
            [
                $"10.1.{server.Number}.0/24",
                $"fc00::1:{server.Number}:0/112"
            ];

        if(!result.Interface.Dns.Any())
            result.Interface.Dns =
            [
                $"10.1.{server.Number}.2",
                $"fc00::1:{server.Number}:2"
            ];

        if (result.Port == default)
            result.Port = 61000 + server.Number;

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");

        if (result.PreSharedKey.IsNullOrEmpty())
            result.PreSharedKey = _wg.GeneratePreSharedKey() ?? throw new("Failed to generate WireGuard pre-shared key");

        if (!result.AllowedIPs.Any())
            result.AllowedIPs = result.Interface.Subnets;

        if (result.Endpoint.IsNullOrEmpty())
            result.Endpoint = GetEndpoint(entity, server);

        return result;
    }

    private string GetEndpoint(IHasWireGuardClient entity, Server server)
    {
        bool isExternal = entity is not Instance instance
                          || instance.Server != server.Name;
        Interface iface = isExternal
            ? server
                  .Interfaces
                  .FirstOrDefault(x => x.Type == NetworkType.Nic)
              ?? throw new($"NIC not found for server: {server.Name}.")
            : server
                  .Interfaces
                  .FirstOrDefault(x => x.Type == NetworkType.Bridge && x.Server == server.Name)
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
