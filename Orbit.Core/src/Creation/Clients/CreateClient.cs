using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Creation.Clients;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine client.
/// </summary>
public class CreateClient : IActivity<Client, Client>
{
    private readonly ILogger<CreateClient> _logger;
    private readonly IEntityProvider<Client> _clients;
    private readonly ClientFactory _factory;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="CreateClient"/>.
    /// </summary>
    public CreateClient(
        ILogger<CreateClient> logger,
        IEntityProvider<Client> clients,
        ClientFactory factory,
        CommandContext context)
    {
        _logger = logger;
        _clients = clients;
        _factory = factory;
        _context = context;
    }

    /// <inheritdoc/>
    public Task<Client> Execute(Client client)
    {
        client = _factory.Create(client);
        if (!client.TryValidate(_logger))
            return Failure(client);
        if (!_clients.Put(client))
            return Failure(client, "Failed to write the client file.");
        _logger.LogInformation($"Created client {client.Name}");
        return Success(client);
    }

    private Task<Client> Success(Client client)
    {
        _context.ExitCode = 0;
        return Task.FromResult(client);
    }

    private Task<Client> Failure(Client client, string error = "", int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(client);
    }
}
