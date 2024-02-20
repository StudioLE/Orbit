using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Creation.Clients;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine client.
/// </summary>
public class CreateClient : IActivity<Client, Client?>
{
    private readonly ILogger<CreateClient> _logger;
    private readonly IEntityProvider<Client> _clients;
    private readonly ClientFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="CreateClient"/>.
    /// </summary>
    public CreateClient(ILogger<CreateClient> logger, IEntityProvider<Client> clients, ClientFactory factory)
    {
        _logger = logger;
        _clients = clients;
        _factory = factory;
    }

    /// <inheritdoc/>
    public Task<Client?> Execute(Client client)
    {
        Func<bool>[] steps =
        {
            () => UpdateClientProperties(ref client),
            () => ValidateClient(client),
            () => PutClient(client)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Created client {client.Name}");
            return Task.FromResult<Client?>(client);
        }
        _logger.LogError("Failed to create client.");
        return Task.FromResult<Client?>(null);
    }

    private bool UpdateClientProperties(ref Client client)
    {
        client = _factory.Create(client);
        return true;
    }

    private bool ValidateClient(Client client)
    {
        return client.TryValidate(_logger);
    }

    private bool PutClient(Client client)
    {
        if (_clients.Put(client))
            return true;
        _logger.LogError("Failed to write the client file.");
        return false;
    }
}
