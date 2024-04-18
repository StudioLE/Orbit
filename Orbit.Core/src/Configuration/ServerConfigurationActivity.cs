using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Instances;
using Orbit.Lxd;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Servers;
using Orbit.Utils.CommandLine;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Configuration;

/// <summary>
/// An <see cref="IActivity"/> to execute stored server configuration files on a server.
/// </summary>
public class ServerConfigurationActivity : IActivity<ServerConfigurationActivity.Inputs, ServerConfigurationActivity.Outputs>
{
    private readonly ILogger<ServerConfigurationActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly ServerProvider _servers;
    private readonly ServerConfigurationProvider _provider;
    private readonly Ssh _ssh;

    /// <summary>
    /// DI constructor for <see cref="ServerConfigurationActivity"/>.
    /// </summary>
    public ServerConfigurationActivity(
        ILogger<ServerConfigurationActivity> logger,
        InstanceProvider instances,
        ServerProvider servers,
        ServerConfigurationProvider provider,
        Ssh ssh)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _provider = provider;
        _ssh = ssh;
    }

    /// <summary>
    /// The inputs for <see cref="ServerConfigurationActivity"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance to launch.
        /// </summary>
        [Required]
        [NameSchema]
        public InstanceId Instance { get; set; }
    }

    /// <summary>
    /// The outputs for <see cref="ServerConfigurationActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }
    }

    /// <inheritdoc/>
    public async Task<Outputs> Execute(Inputs inputs)
    {
        Instance? instanceQuery = await _instances.Get(inputs.Instance);
        if (instanceQuery is not Instance instance)
            return Failure(HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        ServerConfiguration? config = await _provider.Get(instance.Name);
        if (config is null)
            return Failure(HttpStatusCode.NotFound, "Failed to deserialize server configuration");
        foreach ((string serverName, ShellCommand[] commands) in config)
        {
            Server? serverQuery = await _servers.Get(new(serverName));
            if (serverQuery is not Server server)
                return Failure(HttpStatusCode.NotFound, "Failed to get server");
            _ssh.SetServer(server);
            foreach (ShellCommand command in commands)
            {
                int exitCode = _ssh.Execute(command.Command, string.Empty);
                if (exitCode != 0)
                    return Failure(HttpStatusCode.BadRequest, command.ErrorMessage ?? "Failed to run command on server.");
                if (command.SuccessMessage is not null)
                    _logger.LogInformation(command.SuccessMessage);
            }
        }
        _logger.LogInformation($"Initialized instance {instance.Name}");
        return Success();
    }

    private Outputs Success()
    {
        return new()
        {
            Status = new(HttpStatusCode.Created)
        };
    }

    private Outputs Failure(HttpStatusCode statusCode, string error = "")
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode)
        };
    }
}
