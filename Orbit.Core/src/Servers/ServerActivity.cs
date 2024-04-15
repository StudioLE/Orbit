using Microsoft.Extensions.Logging;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Servers;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine server.
/// </summary>
public class ServerActivity : IActivity<Server, Server>
{
    private readonly ILogger<ServerActivity> _logger;
    private readonly ServerProvider _servers;
    private readonly ServerFactory _factory;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="ServerActivity"/>.
    /// </summary>
    public ServerActivity(
        ILogger<ServerActivity> logger,
        ServerProvider servers,
        ServerFactory factory,
        CommandContext context)
    {
        _logger = logger;
        _servers = servers;
        _factory = factory;
        _context = context;
    }

    /// <inheritdoc/>
    public Task<Server> Execute(Server server)
    {
        server = _factory.Create(server);
        if (!server.TryValidate(_logger))
            return Failure(server);
        if (!_servers.Put(server))
            return Failure(server, "Failed to write the server file.");
        _logger.LogInformation($"Created server {server.Name}");
        return Success(server);
    }

    private Task<Server> Success(Server server)
    {
        _context.ExitCode = 0;
        return Task.FromResult(server);
    }

    private Task<Server> Failure(Server server, string error = "", int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(server);
    }
}
