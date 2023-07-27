using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Serialization;
using StudioLE.Core.Serialization;

namespace Orbit.Core.Generation;

/// <summary>
/// An <see cref="IActivity"/> to remotely launch an instance with Multipass.
/// </summary>
public class GenerateServerConfiguration : IActivity<GenerateServerConfiguration.Inputs, GenerateServerConfiguration.Outputs>
{
    public const string FileName = "server-config.yml";
    private readonly ILogger<GenerateServerConfiguration> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Network> _networks;
    private readonly CommandContext _context;
    private readonly WriteCaddyfileCommandFactory _writeCaddyfileCommandFactory;
    private readonly WireGuardSetCommandFactory _wireGuardSetCommandFactory;
    private readonly CloneRepoCommandFactory _cloneRepoCommandFactory;
    private readonly ISerializer _serializer;

    /// <summary>
    /// DI constructor for <see cref="GenerateServerConfiguration"/>.
    /// </summary>
    public GenerateServerConfiguration(
        ILogger<GenerateServerConfiguration> logger,
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
    /// The inputs for <see cref="GenerateServerConfiguration"/>.
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
    /// The outputs of <see cref="GenerateServerConfiguration"/>.
    /// </summary>
    public class Outputs
    {
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
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
            return Task.FromResult(new Outputs());
        }
        _logger.LogError("Failed to generate server configuration");
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

    private bool SetWireGuardPeer(Instance instance, List<KeyValuePair<string, ShellCommand>> commands)
    {
        foreach (WireGuard wg in instance.WireGuard)
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
        if (!_instances.PutResource(new InstanceId(instance.Name), FileName, serialized))
        {
            _logger.LogError("Failed to write server configuration.");
            return false;
        }
        return true;
    }
}
