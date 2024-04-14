using Microsoft.Extensions.Logging;
using Orbit.Creation.Instances;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Caddy;

public class CaddyfileFactory : IFactory<Instance, string?>
{
    private readonly ILogger<CaddyfileFactory> _logger;
    private readonly IEntityProvider<Server> _servers;
    private readonly InternalInterfaceFactory _internalInterfaceFactory;

    /// <summary>
    /// DI constructor for <see cref="CaddyfileFactory"/>.
    /// </summary>
    public CaddyfileFactory(
        ILogger<CaddyfileFactory> logger,
        IEntityProvider<Server> servers,
        InternalInterfaceFactory internalInterfaceFactory)
    {
        _logger = logger;
        _servers = servers;
        _internalInterfaceFactory = internalInterfaceFactory;
    }

    /// <inheritdoc/>
    public string? Create(Instance instance)
    {
        if (instance.Domains.Length == 0)
        {
            _logger.LogWarning("Domains are required for Caddyfile generation.");
            return null;
        }
        string domains = instance.Domains.Join(", ");

        Server server = _servers.Get(instance.Server) ?? throw new($"Server not found: {instance.Server}.");
        string address = _internalInterfaceFactory.GetIPv4Address(instance, server) + ":80";
        return $$"""
            {{domains}} {
                reverse_proxy {{address}}
            }
            """;
    }
}
