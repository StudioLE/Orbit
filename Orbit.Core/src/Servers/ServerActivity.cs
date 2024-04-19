using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Servers;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine server.
/// </summary>
public class ServerActivity : ActivityBase<Server, ServerActivity.Outputs>
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
    public override async Task<Outputs?> Execute(Server server)
    {
        server = await _factory.Create(server);
        if (!server.TryValidate(_logger))
            return Failure(server, HttpStatusCode.BadRequest);
        if (!await _servers.Put(server))
            return Failure(server, HttpStatusCode.InternalServerError, "Failed to write the server file.");
        _logger.LogInformation($"Created server {server.Name}");
        return Success(server);
    }

    private Outputs Success(Server server)
    {
        return new()
        {
            Status = new(HttpStatusCode.Created),
            Server = server
        };
    }

    private Outputs Failure(Server server, HttpStatusCode statusCode, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode),
            Server = server
        };
    }
}
