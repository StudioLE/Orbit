using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Servers;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine server.
/// </summary>
public class ServerActivity : IActivity<Server, ServerActivity.Outputs>
{
    private readonly ILogger<ServerActivity> _logger;
    private readonly ServerProvider _servers;
    private readonly ServerFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="ServerActivity"/>.
    /// </summary>
    public ServerActivity(
        ILogger<ServerActivity> logger,
        ServerProvider servers,
        ServerFactory factory)
    {
        _logger = logger;
        _servers = servers;
        _factory = factory;
    }

    /// <summary>
    /// The outputs for <see cref="ServerActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The server.
        /// </summary>
        public Server Server { get; set; }
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Server server)
    {
        server = _factory.Create(server);
        if (!server.TryValidate(_logger))
            return Failure(server, HttpStatusCode.BadRequest);
        if (!_servers.Put(server))
            return Failure(server, HttpStatusCode.InternalServerError, "Failed to write the server file.");
        _logger.LogInformation($"Created server {server.Name}");
        return Success(server);
    }

    private Task<Outputs> Success(Server server)
    {
        Outputs outputs = new()
        {
            Status = new(HttpStatusCode.Created),
            Server = server
        };
        return Task.FromResult(outputs);
    }

    private Task<Outputs> Failure(Server server, HttpStatusCode statusCode, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        Outputs outputs = new()
        {
            Status = new(statusCode),
            Server = server
        };
        return Task.FromResult(outputs);
    }
}
