using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils.DataAnnotations;
using Renci.SshNet;

namespace Orbit.Core.Initialization;

/// <summary>
/// An <see cref="IActivity"/> to remotely launch an instance with Multipass.
/// </summary>
public class Mount : IActivity<Mount.Inputs, Mount.Outputs>
{
    private readonly ILogger<Mount> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;

    /// <summary>
    /// DI constructor for <see cref="Mount"/>.
    /// </summary>
    public Mount(ILogger<Mount> logger, IEntityProvider<Instance> instances, IEntityProvider<Server> servers)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
    }

    /// <summary>
    /// The inputs for <see cref="Mount"/>.
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
    /// The outputs of <see cref="Mount"/>.
    /// </summary>
    public class Outputs
    {
        public int ExitCode { get; set; }
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
    {
        Instance instance = new();
        Func<bool>[] steps =
        {
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => CreateMountsOnServer(instance)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Created mounts for instance {instance.Name}");
            return Task.FromResult(new Outputs { ExitCode = 0 });
        }
        _logger.LogError("Failed to create mounts for instance.");
        return Task.FromResult(new Outputs { ExitCode = 1 });
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

    private bool CreateMountsOnServer(Instance instance)
    {
        if (!instance.Mounts.Any())
            return true;
        Server? server = _servers.Get(new ServerId(instance.Server));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        foreach (Schema.Mount mount in instance.Mounts)
        {
            if (!mount.Source.StartsWith("/mnt"))
            {
                _logger.LogError($"Mount source path is invalid: {mount.Source}");
                return false;
            }
            string mkdirCommand = $"mkdir -p {mount.Source}";
            if (!ssh.ExecuteToLogger(_logger, mkdirCommand))
            {
                _logger.LogError("Failed to create mount directory on server.");
                return false;
            }
            string multipassCommand = $"multipass mount {mount.Source} {instance.Name}:{mount.Target}";
            if (!ssh.ExecuteToLogger(_logger, multipassCommand))
            {
                _logger.LogError($"Failed to create mount: {instance.Name}:{mount.Target}");
                return false;
            }
        }
        return true;
    }
}
