using Microsoft.Extensions.Logging;
using StudioLE.Orbit.Schema;
using StudioLE.Patterns;

namespace StudioLE.Orbit.Caddy;

/// <summary>
/// Create a <see cref="ShellCommand"/> to write a Caddyfile using <see cref="CaddyfileFactory"/> to a server.
/// </summary>
public class WriteCaddyfileCommandFactory : IFactory<Instance, Task<ShellCommand[]>>
{
    private readonly ILogger<WriteCaddyfileCommandFactory> _logger;
    private readonly CaddyfileFactory _factory;

    /// <summary>
    /// Create a new instance of <see cref="WriteCaddyfileCommandFactory"/>.
    /// </summary>
    public WriteCaddyfileCommandFactory(ILogger<WriteCaddyfileCommandFactory> logger, CaddyfileFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    /// <inheritdoc/>
    public async Task<ShellCommand[]> Create(Instance instance)
    {
        string? caddyfile = await _factory.Create(instance);
        if (caddyfile is null)
        {
            _logger.LogError("Failed to create Caddyfile");
            return Array.Empty<ShellCommand>();
        }
        return
        [
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
        ];
    }
}
