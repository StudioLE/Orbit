using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Caddy;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Orbit.WireGuard;
using StudioLE.Serialization;

namespace Orbit.Configuration;

/// <summary>
/// An <see cref="IActivity"/> to generate the server configuration for an instance.
/// </summary>
public class GenerateServerConfigurationForInstance : IActivity<GenerateServerConfigurationForInstance.Inputs, string>
{
    public const string FileName = "server-config.yml";
    private readonly ILogger<GenerateServerConfigurationForInstance> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly CommandContext _context;
    private readonly WriteCaddyfileCommandFactory _writeCaddyfileCommandFactory;
    private readonly WireGuardSetCommandFactory _wireGuardSetCommandFactory;
    private readonly ISerializer _serializer;

    /// <summary>
    /// DI constructor for <see cref="GenerateServerConfigurationForInstance"/>.
    /// </summary>
    public GenerateServerConfigurationForInstance(
        ILogger<GenerateServerConfigurationForInstance> logger,
        IEntityProvider<Instance> instances,
        CommandContext context,
        WriteCaddyfileCommandFactory writeCaddyfileCommandFactory,
        WireGuardSetCommandFactory wireGuardSetCommandFactory,
        ISerializer serializer)
    {
        _logger = logger;
        _instances = instances;
        _context = context;
        _writeCaddyfileCommandFactory = writeCaddyfileCommandFactory;
        _wireGuardSetCommandFactory = wireGuardSetCommandFactory;
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
        Instance? instanceQuery = _instances.Get(new InstanceId(inputs.Instance));
        if (instanceQuery is not Instance instance)
            return Failure("The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure();
        List<KeyValuePair<ServerId, ShellCommand>> commands = new();
        if (!SetWireGuardPeer(instance, commands))
            return Failure();
        if (!WriteCaddyfile(instance, commands))
            return Failure();
        // Write configuration
        Dictionary<ServerId, ShellCommand[]> dictionary = commands
            .GroupBy(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());
        string serialized = _serializer.Serialize(dictionary);
        if (!_instances.PutResource(instance.Name, FileName, serialized))
            return Failure("Failed to write server configuration.");
        return Success(string.Empty);
    }

    private bool SetWireGuardPeer(Instance instance, List<KeyValuePair<ServerId, ShellCommand>> commands)
    {
        foreach (WireGuardClient wg in instance.WireGuard)
        {
            ShellCommand command = _wireGuardSetCommandFactory.Create(wg);
            commands.Add(new(wg.Interface.Server, command));
        }
        return true;
    }

    private bool WriteCaddyfile(Instance instance, List<KeyValuePair<ServerId, ShellCommand>> commands)
    {
        if (!instance.Domains.Any())
            return true;
        ShellCommand[] results = _writeCaddyfileCommandFactory.Create(instance);
        if (!results.Any())
            return false;
        commands.AddRange(results
            .Select(x => new KeyValuePair<ServerId, ShellCommand>(instance.Server, x)));
        return true;
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
