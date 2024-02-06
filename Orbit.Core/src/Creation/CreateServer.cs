using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Creation;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine server.
/// </summary>
public class CreateServer : IActivity<Server, Server?>
{
    private readonly ILogger<CreateServer> _logger;
    private readonly IEntityProvider<Server> _servers;
    private readonly ServerFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="CreateServer"/>.
    /// </summary>
    public CreateServer(ILogger<CreateServer> logger, IEntityProvider<Server> servers, ServerFactory factory)
    {
        _logger = logger;
        _servers = servers;
        _factory = factory;
    }

    /// <inheritdoc/>
    public Task<Server?> Execute(Server server)
    {
        Func<bool>[] steps =
        {
            () => UpdateServerProperties(ref server),
            () => ValidateServer(server),
            () => PutServer(server)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Created server {server.Name}");
            return Task.FromResult<Server?>(server);
        }
        _logger.LogError("Failed to create server.");
        return Task.FromResult<Server?>(null);
    }

    private bool UpdateServerProperties(ref Server server)
    {
        server = _factory.Create(server);
        return true;
    }

    private bool ValidateServer(Server server)
    {
        return server.TryValidate(_logger);
    }

    private bool PutServer(Server server)
    {
        if (_servers.Put(server))
            return true;
        _logger.LogError("Failed to write the server file.");
        return false;
    }
}
