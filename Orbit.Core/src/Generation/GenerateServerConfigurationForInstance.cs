using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using StudioLE.Serialization;

namespace Orbit.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the server configuration for an instance.
/// </summary>
public class GenerateServerConfigurationForInstance : IActivity<GenerateServerConfigurationForInstance.Inputs, GenerateServerConfiguration.Outputs>
{
    private readonly ILogger<GenerateServerConfigurationForInstance> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Network> _networks;
    private readonly CommandContext _context;
    private readonly WriteCaddyfileCommandFactory _writeCaddyfileCommandFactory;
    private readonly WireGuardSetCommandFactory _wireGuardSetCommandFactory;
    private readonly CloneRepoCommandFactory _cloneRepoCommandFactory;
    private readonly ISerializer _serializer;

    /// <summary>
    /// DI constructor for <see cref="GenerateServerConfigurationForInstance"/>.
    /// </summary>
    public GenerateServerConfigurationForInstance(
        ILogger<GenerateServerConfigurationForInstance> logger,
        IEntityProvider<Instance> instances,
        IEntityProvider<Network> networks,
        CommandContext context,
        WriteCaddyfileCommandFactory writeCaddyfileCommandFactory,
        WireGuardSetCommandFactory wireGuardSetCommandFactory,
        CloneRepoCommandFactory cloneRepoCommandFactory,
        ISerializer serializer)
    {
        _logger = logger;
        _instances = instances;
        _networks = networks;
        _context = context;
        _writeCaddyfileCommandFactory = writeCaddyfileCommandFactory;
        _wireGuardSetCommandFactory = wireGuardSetCommandFactory;
        _cloneRepoCommandFactory = cloneRepoCommandFactory;
        _serializer = serializer;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateServerConfigurationForInstance"/>.
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
    public Task<GenerateServerConfiguration.Outputs> Execute(Inputs inputs)
    {
        Instance instance = new();
        List<KeyValuePair<string, ShellCommand>> commands = new();
        Func<bool>[] steps =
        {
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => SetWireGuardPeer(instance, commands),
            () => WriteCaddyfile(instance, commands),
            () => CloneRepo(instance, commands),
            () => WriteConfiguration(instance, commands)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation("Generated server configuration");
            return Task.FromResult(new GenerateServerConfiguration.Outputs());
        }
        _logger.LogError("Failed to generate server configuration");
        _context.ExitCode = 1;
        return Task.FromResult(new GenerateServerConfiguration.Outputs());
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

    private bool SetWireGuardPeer(Instance instance, List<KeyValuePair<string, ShellCommand>> commands)
    {
        foreach (WireGuardClient wg in instance.WireGuard)
        {
            Network? network = _networks.Get(new NetworkId(wg.Network));
            if (network is null)
            {
                _logger.LogError("Failed to get network");
                return false;
            }
            ShellCommand command = _wireGuardSetCommandFactory.Create(wg);
            commands.Add(new(network.Server, command));
        }
        return true;
    }

    private bool WriteCaddyfile(Instance instance, List<KeyValuePair<string, ShellCommand>> commands)
    {
        if (!instance.Domains.Any())
        {
            _logger.LogWarning("Domains are required for Caddyfile generation.");
            return true;
        }
        ShellCommand[] results = _writeCaddyfileCommandFactory.Create(instance);
        if (!results.Any())
            return false;
        commands.AddRange(results
            .Select(x => new KeyValuePair<string, ShellCommand>(instance.Server, x)));
        return true;
    }

    private bool CloneRepo(Instance instance, List<KeyValuePair<string, ShellCommand>> commands)
    {
        ShellCommand? command = _cloneRepoCommandFactory.Create(instance);
        if (command is null)
            return true;
        commands.Add(new(instance.Server, command));
        return true;
    }

    private bool WriteConfiguration(Instance instance, List<KeyValuePair<string, ShellCommand>> commands)
    {
        Dictionary<string, ShellCommand[]> dictionary = commands
            .GroupBy(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());

        string serialized = _serializer.Serialize(dictionary);
        if (!_instances.PutResource(new InstanceId(instance.Name), GenerateServerConfiguration.FileName, serialized))
        {
            _logger.LogError("Failed to write server configuration.");
            return false;
        }
        return true;
    }
}
