using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils.DataAnnotations;
using Renci.SshNet;

namespace Orbit.Core.Activities;

/// <summary>
/// An <see cref="IActivity"/> to remotely launch an instance with Multipass.
/// </summary>
public class Launch : IActivity<Launch.Inputs, Launch.Outputs>
{
    private readonly ILogger<Launch> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly CommandContext _context;
    private readonly LaunchCommandFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="Launch"/>.
    /// </summary>
    public Launch(
        ILogger<Launch> logger,
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        CommandContext context,
        LaunchCommandFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="Launch"/>.
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
    /// The outputs of <see cref="Launch"/>.
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
            () => LaunchOnServer(instance)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Launched instance {instance.Name}");
            return Task.FromResult(new Outputs());
        }
        _logger.LogError("Failed to launch instance.");
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

    private bool LaunchOnServer(Instance instance)
    {
        Server? server = _servers.Get(new ServerId(instance.Server));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        string? command = _factory.Create(instance);
        if (command is null)
            return false;
        if (ssh.ExecuteToLogger(_logger, command))
            return true;
        _logger.LogError("Failed to run multipass launch on server.");
        return false;
    }
}
