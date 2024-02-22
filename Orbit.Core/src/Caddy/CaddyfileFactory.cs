using Microsoft.Extensions.Logging;
using Orbit.Networking;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Caddy;

public class CaddyfileFactory : IFactory<Instance, string?>
{
    private readonly ILogger<CaddyfileFactory> _logger;
    private readonly IEntityProvider<Network> _networks;
    private readonly IIPAddressStrategy _ip;

    /// <summary>
    /// DI constructor for <see cref="CaddyfileFactory"/>.
    /// </summary>
    public CaddyfileFactory(ILogger<CaddyfileFactory> logger, IEntityProvider<Network> networks, IIPAddressStrategy ip)
    {
        _logger = logger;
        _networks = networks;
        _ip = ip;
    }

    /// <inheritdoc />
    public string? Create(Instance instance)
    {
        if (!instance.Domains.Any())
        {
            _logger.LogWarning("Domains are required for Caddyfile generation.");
            return null;
        }
        Network? network = _networks.Get(new NetworkId(instance.Networks.First()));
        if (network is null)
        {
            _logger.LogError("The network does not exist.");
            return null;
        }
        string domains = instance.Domains.Join(", ");
        string address = instance.Server == network.Server
            ? _ip.GetInternalIPv4(instance, network) + ":80"
            : _ip.GetWireGuardIPv4(instance, network) + ":80";
        return $$"""
            {{domains}} {
                reverse_proxy {{address}}
            }
            """;
    }
}
