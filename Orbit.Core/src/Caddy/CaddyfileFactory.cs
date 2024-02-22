using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.Networking;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Caddy;

public class CaddyfileFactory : IFactory<Instance, string?>
{
    private readonly ILogger<CaddyfileFactory> _logger;

    /// <summary>
    /// DI constructor for <see cref="CaddyfileFactory"/>.
    /// </summary>
    public CaddyfileFactory(ILogger<CaddyfileFactory> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string? Create(Instance instance)
    {
        if (!instance.Domains.Any())
        {
            _logger.LogWarning("Domains are required for Caddyfile generation.");
            return null;
        }
        // TODO: We have no idea what server the domain is point at so this is irrelevant
        Interface iface = true
            ? instance
                .Interfaces
                .First(x => x.Type == NetworkType.Bridge)
            : instance
                .Interfaces
                .First(x => x.Type == NetworkType.WireGuard);
        string domains = instance.Domains.Join(", ");
        string address = IPHelpers.RemoveCidr(iface.Addresses.First()) + ":80";
        return $$"""
            {{domains}} {
                reverse_proxy {{address}}
            }
            """;
    }
}
