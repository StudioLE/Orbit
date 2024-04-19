using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Servers;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine servers.
/// </summary>
public class ServerUpdateActivity : ActivityBase<ServerUpdateActivity.Inputs, ServerUpdateActivity.Outputs>
{
    private readonly ILogger<ServerUpdateActivity> _logger;
    private readonly ServerProvider _instances;
    private readonly ServerFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="ServerUpdateActivity"/>.
    /// </summary>
    public ServerUpdateActivity(
        ILogger<ServerUpdateActivity> logger,
        ServerProvider instances,
        ServerFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="ServerUpdateActivity"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance to launch.
        /// </summary>
        [Required]
        [NameSchema]
        [Argument]
        public ServerId Server { get; set; }
    }

    /// <summary>
    /// The outputs for <see cref="ServerUpdateActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The instance.
        /// </summary>
        public Server? Server { get; set; }
    }

    /// <inheritdoc/>
    public override async Task<Outputs?> Execute(Inputs inputs)
    {
        Server? instanceQuery = await _instances.Get(inputs.Server);
        if (instanceQuery is not Server instance)
            return Failure(null, HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(instance, HttpStatusCode.BadRequest);
        if (!await _instances.Put(instance))
            return Failure(instance, HttpStatusCode.InternalServerError, "Failed to write the instance file.");
        _logger.LogInformation($"Created instance {instance.Name}");
        return Success(instance);
    }

    private Outputs Success(Server instance)
    {
        return new()
        {
            Status = new(HttpStatusCode.Created),
            Server = instance
        };
    }

    private Outputs Failure(Server? instance, HttpStatusCode statusCode, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode),
            Server = instance
        };
    }
}
