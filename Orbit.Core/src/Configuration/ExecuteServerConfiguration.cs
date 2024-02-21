using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Lxd;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.CommandLine;
using Orbit.Utils.DataAnnotations;
using StudioLE.Serialization;

namespace Orbit.Configuration;

/// <summary>
/// An <see cref="IActivity"/> to configure a server.
/// </summary>
public class ExecuteServerConfiguration : IActivity<ExecuteServerConfiguration.Inputs, string>
{
    private readonly ILogger<ExecuteServerConfiguration> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly CommandContext _context;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="ExecuteServerConfiguration"/>.
    /// </summary>
    public ExecuteServerConfiguration(
        ILogger<ExecuteServerConfiguration> logger,
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        CommandContext context,
        IDeserializer deserializer)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _context = context;
        _deserializer = deserializer;
    }

    /// <summary>
    /// The inputs for <see cref="ExecuteServerConfiguration"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance to launch.
        /// </summary>
        [Required]
        [NameSchema]
        public string Instance { get; set; } = string.Empty;
    }

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        Instance? instance = _instances.Get(new InstanceId(inputs.Instance));
        if (instance is null)
            return Failure("The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure();
        string? resource = _instances.GetResource(new InstanceId(instance.Name), GenerateServerConfigurationForInstance.FileName);
        if (resource is null)
            return Failure("Failed to get server configuration");
        ServerConfiguration? config = _deserializer.Deserialize<ServerConfiguration>(resource);
        if (config is null)
            return Failure("Failed to deserialize server configuration");
        foreach ((string serverName, ShellCommand[] commands) in config)
        {
            Server? server = _servers.Get(new ServerId(serverName));
            if (server is null)
                return Failure("Failed to get server");
            Ssh ssh = LxdHelpers.CreateSsh(_logger, server);
            foreach (ShellCommand command in commands)
            {
                int exitCode = ssh.Execute(command.Command, string.Empty);
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
