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
public class GenerateServerConfigurationForInstance : IActivity<GenerateServerConfigurationForInstance.Inputs, string>
{
    public const string FileName = "server-config.yml";
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
        [Argument]
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
        List<KeyValuePair<string, ShellCommand>> commands = new();
        if (!SetWireGuardPeer(instance, commands))
            return Failure();
        if (!WriteCaddyfile(instance, commands))
            return Failure();
        if (!CloneRepo(instance, commands))
            return Failure();
        // Write configuration
        Dictionary<string, ShellCommand[]> dictionary = commands
            .GroupBy(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());
        string serialized = _serializer.Serialize(dictionary);
        if (!_instances.PutResource(new InstanceId(instance.Name), FileName, serialized))
            return Failure("Failed to write server configuration.");
        return Success(string.Empty);
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
            return true;
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

    private Task<string> Success(string output)
    {
        _context.ExitCode = 0;
        return Task.FromResult(output);
    }

    private Task<string> Failure(string error = "", int exitCode = 1)
    {
        if(!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(error);
    }
}
