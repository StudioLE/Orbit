using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orbit.Instances;
using Orbit.Lxd;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Servers;
using Orbit.Utils.CommandLine;
using Orbit.Utils.DataAnnotations;
using StudioLE.Serialization;
using Tectonic;

namespace Orbit.Configuration;

/// <summary>
/// An <see cref="IActivity"/> to execute stored server configuration files on a server.
/// </summary>
public class ServerConfigurationActivity : IActivity<ServerConfigurationActivity.Inputs, string>
{
    private readonly ILogger<ServerConfigurationActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly ServerProvider _servers;
    private readonly ServerConfigurationProvider _provider;
    private readonly CommandContext _context;
    private readonly IDeserializer _deserializer;
    private readonly Ssh _ssh;

    /// <summary>
    /// DI constructor for <see cref="ServerConfigurationActivity"/>.
    /// </summary>
    public ServerConfigurationActivity(
        ILogger<ServerConfigurationActivity> logger,
        InstanceProvider instances,
        ServerProvider servers,
        ServerConfigurationProvider provider,
        CommandContext context,
        IDeserializer deserializer,
        Ssh ssh)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _provider = provider;
        _context = context;
        _deserializer = deserializer;
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

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        Instance? instanceQuery = _instances.Get(inputs.Instance);
        if (instanceQuery is not Instance instance)
            return Failure("The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure();
        ServerConfiguration? config = _provider.Get(instance.Name);
        if (config is null)
            return Failure("Failed to deserialize server configuration");
        foreach ((string serverName, ShellCommand[] commands) in config)
        {
            Server? serverQuery = _servers.Get(new ServerId(serverName));
            if (serverQuery is not Server server)
                return Failure("Failed to get server");
            _ssh.SetServer(server);
            foreach (ShellCommand command in commands)
            {
                int exitCode = _ssh.Execute(command.Command, string.Empty);
                if (exitCode != 0)
                    return Failure(command.ErrorMessage ?? "Failed to run command on server.");
                if (command.SuccessMessage is not null)
                    _logger.LogInformation(command.SuccessMessage);
            }
        }
        _logger.LogInformation($"Initialized instance {instance.Name}");
        return Success(string.Empty);
    }

    private Task<string> Success(string output)
    {
        _context.ExitCode = 0;
        return Task.FromResult(output);
    }

    private Task<string> Failure(string error = "", int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(error);
    }
}
