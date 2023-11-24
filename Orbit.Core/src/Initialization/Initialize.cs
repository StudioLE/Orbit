using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils.DataAnnotations;
using Renci.SshNet;
using StudioLE.Serialization;

namespace Orbit.Core.Initialization;

/// <summary>
/// An <see cref="IActivity"/> to remotely launch an instance with Multipass.
/// </summary>
public class Initialize : IActivity<Initialize.Inputs, Initialize.Outputs>
{
    private readonly ILogger<Initialize> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly CommandContext _context;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="Initialize"/>.
    /// </summary>
    public Initialize(
        ILogger<Initialize> logger,
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
    /// The inputs for <see cref="Initialize"/>.
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

    /// <summary>
    /// The outputs of <see cref="Initialize"/>.
    /// </summary>
    public class Outputs
    {
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
    {
        Instance instance = new();
        Func<bool>[] steps =
        {
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => InitializeOnServer(instance)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Initialized instance {instance.Name}");
            return Task.FromResult(new Outputs());
        }
        _logger.LogError("Failed to initialize instance.");
        _context.ExitCode = 1;
        return Task.FromResult(new Outputs());
    }

    private bool GetInstance(string instanceName, out Instance instance)
    {
        Instance? result = _instances.Get(new InstanceId(instanceName));
        instance = result!;
        if (result is null)
        {
            _logger.LogError("The instance does not exist.");
            return false;
        }
        return true;
    }

    private bool ValidateInstance(Instance instance)
    {
        return instance.TryValidate(_logger);
    }

    private bool InitializeOnServer(Instance instance)
    {
        string? resource = _instances.GetResource(new InstanceId(instance.Name), GenerateServerConfiguration.FileName);
        if (resource is null)
        {
            _logger.LogError("Failed to get server configuration");
            return false;
        }
        ServerConfiguration? config = _deserializer.Deserialize<ServerConfiguration>(resource);
        if (config is null)
        {
            _logger.LogError("Failed to deserialize server configuration");
            return false;
        }
        return config.All(pair => InitializeOnServer(pair.Key, pair.Value));
    }

    private bool InitializeOnServer(string serverName, ShellCommand[] commands)
    {
        Server? server = _servers.Get(new ServerId(serverName));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        foreach (ShellCommand command in commands)
        {
            if (!ssh.ExecuteToLogger(_logger, command.Command))
            {
                string errorMessage = command.ErrorMessage ?? "Failed to run command on server.";
                _logger.LogError(errorMessage);
                return false;
            }
            if (command.SuccessMessage is not null)
                _logger.LogInformation(command.SuccessMessage);
        }
        ssh.Disconnect();
        return true;
    }
}
