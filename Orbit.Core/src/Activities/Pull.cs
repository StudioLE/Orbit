using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
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
public class Pull : IActivity<Pull.Inputs, Pull.Outputs>
{
    private readonly ILogger<Pull> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;

    /// <summary>
    /// DI constructor for <see cref="Pull"/>.
    /// </summary>
    public Pull(ILogger<Pull> logger, IEntityProvider<Instance> instances, IEntityProvider<Server> servers)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
    }

    /// <summary>
    /// The inputs for <see cref="Pull"/>.
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
    /// The outputs of <see cref="Pull"/>.
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
            () => PullRepoOnServer(instance)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Pulled repository for instance {instance.Name}");
            return Task.FromResult(new Outputs { ExitCode = 0 });
        }
        _logger.LogError("Failed to pull repository..");
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

    private bool PullRepoOnServer(Instance instance)
    {
        if (instance.Repo is null)
        {
            _logger.LogError("Instance does not have a repo.");
            return false;
        }
        // TODO: Validate repo
        Server? server = _servers.Get(new ServerId(instance.Server));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        string command = $"git clone {instance.Repo.Origin} /mnt/{instance.Name}/srv --branch {instance.Repo.Branch} --depth 1";
        if (!ssh.ExecuteToLogger(_logger, command))
        {
            _logger.LogError("Failed to pull repository.");
            return false;
        }
        return true;
    }
}
