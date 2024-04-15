using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Clients;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine client.
/// </summary>
public class ClientActivity : IActivity<Client, ClientActivity.Outputs>
{
    private readonly ILogger<ClientActivity> _logger;
    private readonly ClientProvider _clients;
    private readonly ClientFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="ClientActivity"/>.
    /// </summary>
    public ClientActivity(
        ILogger<ClientActivity> logger,
        ClientProvider clients,
        ClientFactory factory)
    {
        _logger = logger;
        _clients = clients;
        _factory = factory;
    }

    /// <summary>
    /// The outputs for <see cref="ClientActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The client.
        /// </summary>
        public Client Client { get; set; }
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Client client)
    {
        client = _factory.Create(client);
        if (!client.TryValidate(_logger))
            return Failure(client, HttpStatusCode.BadRequest);
        if (!_clients.Put(client))
            return Failure(client, HttpStatusCode.InternalServerError, "Failed to write the client file.");
        _logger.LogInformation($"Created client {client.Name}");
        return Success(client);
    }

    private Task<Outputs> Success(Client client)
    {
        Outputs outputs = new()
        {
            Status = new(HttpStatusCode.Created),
            Client = client
        };
        return Task.FromResult(outputs);
    }

    private Task<Outputs> Failure(Client client, HttpStatusCode statusCode, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        Outputs outputs = new()
        {
            Status = new(statusCode),
            Client = client
        };
        return Task.FromResult(outputs);
    }
}
