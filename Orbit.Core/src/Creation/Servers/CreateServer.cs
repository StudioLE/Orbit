using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Creation.Servers;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine server.
/// </summary>
public class CreateServer : IActivity<Server, Server>
{
    private readonly ILogger<CreateServer> _logger;
    private readonly IEntityProvider<Server> _servers;
    private readonly ServerFactory _factory;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="CreateServer"/>.
    /// </summary>
    public CreateServer(
        ILogger<CreateServer> logger,
        IEntityProvider<Server> servers,
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
