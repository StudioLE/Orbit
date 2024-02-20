using Microsoft.Extensions.Logging;
using Orbit.Schema;
using StudioLE.Patterns;

namespace Orbit.Caddy;

public class WriteCaddyfileCommandFactory : IFactory<Instance, ShellCommand[]>
{
    private readonly ILogger<WriteCaddyfileCommandFactory> _logger;
    private readonly CaddyfileFactory _factory;

    public WriteCaddyfileCommandFactory(ILogger<WriteCaddyfileCommandFactory> logger, CaddyfileFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public ShellCommand[] Create(Instance instance)
    {
        string? caddyfile = _factory.Create(instance);
        if (caddyfile is null)
        {
            _logger.LogError("Failed to create Caddyfile");
            return Array.Empty<ShellCommand>();
        }
        return new ShellCommand[]
        {
            new()
            {
                Command = $$"""
                    (
                    cat <<'EOF'
                    {{caddyfile}}
                    EOF
                    ) | tee "/caddy/{{instance.Name}}.caddy" > /dev/null
                    """,
                ErrorMessage = "Failed to write Caddyfile"
            },
            new()
            {
                Command = "cd /caddy && sudo caddy reload",
                ErrorMessage = "Failed to reload Caddy"
            }
        };
    }
}
