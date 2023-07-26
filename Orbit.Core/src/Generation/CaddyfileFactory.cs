using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

public class CaddyfileFactory : IFactory<Instance, string?>
{
    private readonly ILogger<CaddyfileFactory> _logger;
    private readonly IEntityProvider<Network> _networks;

    /// <summary>
    /// DI constructor for <see cref="CaddyfileFactory"/>.
    /// </summary>
    public CaddyfileFactory(ILogger<CaddyfileFactory> logger, IEntityProvider<Network> networks)
    {
        _logger = logger;
        _networks = networks;
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
        string address = network.GetInternalIPv4(instance) + ":80";
        return $$"""
            {{domains}} {
                reverse_proxy {{address}}
            }
            """;
    }
}
