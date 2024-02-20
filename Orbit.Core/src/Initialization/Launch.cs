using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Generation;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Shell;
using Orbit.Utils.CommandLine;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Initialization;

/// <summary>
/// An <see cref="IActivity"/> to remotely launch an instance with Multipass.
/// </summary>
public class Launch : IActivity<Launch.Inputs, Launch.Outputs>
{
    private readonly ILogger<Launch> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly IEntityProvider<Network> _networks;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="Launch"/>.
    /// </summary>
    public Launch(
        ILogger<Launch> logger,
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        IEntityProvider<Network> networks,
        CommandContext context)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _context = context;
        _networks = networks;
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
        Ssh ssh = MultipassHelpers.CreateSsh(_logger, server);
        string? cloudInit = _instances.GetResource(new InstanceId(instance.Name), GenerateInstanceConfiguration.FileName);
        if (cloudInit is null)
        {
            _logger.LogError("Failed to get user-config");
            return false;
        }
        string networkName = instance.Networks.FirstOrDefault() ?? throw new("Instance has no networks");
        Network network = _networks.Get(new NetworkId(networkName)) ?? throw new($"Network {networkName} not found");
        string[] args = [
            "launch",
            $"--cpus {instance.Hardware.Cpus}",
            $"--memory {instance.Hardware.Memory}G",
            $"--disk {instance.Hardware.Disk}G",
            $"--name {instance.Name}",
            $"--network name=br{network.Number},mode=manual,mac=\"{instance.MacAddress}\"",
            "--cloud-init -"
        ];
        int exitCode = ssh.Execute("multipass", string.Join(" ", args), cloudInit);
        if (exitCode == 0)
            return true;
        _logger.LogError("Failed to run multipass launch on server.");
        return false;
    }
}
