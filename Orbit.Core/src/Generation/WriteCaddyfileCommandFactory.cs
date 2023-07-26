using Microsoft.Extensions.Logging;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Generation;

public class WriteCaddyfileCommandFactory : IFactory<Instance, string?>
{
    private readonly ILogger<WriteCaddyfileCommandFactory> _logger;
    private readonly CaddyfileFactory _factory;

    public WriteCaddyfileCommandFactory(ILogger<WriteCaddyfileCommandFactory> logger, CaddyfileFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public string? Create(Instance instance)
    {
        string? caddyfile = _factory.Create(instance);
        if (caddyfile is null)
        {
            _logger.LogError("Failed to create Caddyfile");
            return null;
        }
        // TODO: Wrap in bash if
        return $$"""
            (
            cat <<EOF
            {{caddyfile}}
            EOF
            ) | tee /caddy/{{instance.Name}}.caddy

            cd /caddy && sudo caddy reload
            """;
    }
}
