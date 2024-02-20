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
public class Launch : IActivity<Launch.Inputs, string>
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

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        Instance? instance = _instances.Get(new InstanceId(inputs.Instance));
        if (instance is null)
            return Failure("The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure();
        Server? server = _servers.Get(new ServerId(instance.Server));
        if (server is null)
            return Failure("Failed to get server");
        Ssh ssh = MultipassHelpers.CreateSsh(_logger, server);
        string? cloudInit = _instances.GetResource(new InstanceId(instance.Name), GenerateInstanceConfiguration.FileName);
        if (cloudInit is null)
            return Failure("Failed to get user-config");
        string networkName = instance.Networks.FirstOrDefault() ?? throw new("Instance has no networks");
        Network network = _networks.Get(new NetworkId(networkName)) ?? throw new($"Network {networkName} not found");
        string[] args =
        [
            "launch",
            $"--cpus {instance.Hardware.Cpus}",
            $"--memory {instance.Hardware.Memory}G",
            $"--disk {instance.Hardware.Disk}G",
            $"--name {instance.Name}",
            $"--network name=br{network.Number},mode=manual,mac=\"{instance.MacAddress}\"",
            "--cloud-init -"
        ];
        int exitCode = ssh.Execute("multipass", string.Join(" ", args), cloudInit);
        if (exitCode != 0)
            return Failure("Failed to run multipass launch on server.");
        _logger.LogInformation($"Launched instance {instance.Name}");
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
