using Microsoft.Extensions.Logging;
using StudioLE.Orbit.Instances;
using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Servers;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace StudioLE.Orbit.Caddy;

/// <summary>
/// Generate a Caddyfile that reverse proxies requests for <see cref="Instance.Domains"/> from <see cref="Server"/> to <see cref="Instance"/>.
/// </summary>
/// <remarks>
/// Proxying is necessary when the <see cref="Instance"/> is running on a private network and
/// is not directly accessible from the internet.
/// If the <see cref="Instance"/> has a <see cref="NetworkType.RoutedNic"/> this is not necessary.
/// Follows a <see href="https://refactoring.guru/design-patterns/factory-method">factory method pattern</see>.
/// </remarks>
public class CaddyfileFactory : IFactory<Instance, Task<string?>>
{
    private readonly ILogger<CaddyfileFactory> _logger;
    private readonly ServerProvider _servers;
    private readonly InternalInterfaceFactory _internalInterfaceFactory;

    /// <summary>
    /// DI constructor for <see cref="CaddyfileFactory"/>.
    /// </summary>
    public CaddyfileFactory(
        ILogger<CaddyfileFactory> logger,
        ServerProvider servers,
        InternalInterfaceFactory internalInterfaceFactory)
    {
        _logger = logger;
        _servers = servers;
        _internalInterfaceFactory = internalInterfaceFactory;
    }

    /// <inheritdoc/>
    public async Task<string?> Create(Instance instance)
    {
        if (instance.Domains.Length == 0)
        {
            _logger.LogWarning("Domains are required for Caddyfile generation.");
            return null;
        }
        string domains = instance.Domains.Join(", ");

        Server server = await _servers.Get(instance.Server) ?? throw new($"Server not found: {instance.Server}.");
        string address = _internalInterfaceFactory.GetIPv4Address(instance, server) + ":80";
        return $$"""
            {{domains}} {
                reverse_proxy {{address}}
            }
            """;
    }
}
