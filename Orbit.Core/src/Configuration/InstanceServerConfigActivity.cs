using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Caddy;
using Orbit.Instances;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Orbit.WireGuard;
using Tectonic;

namespace Orbit.Configuration;

/// <summary>
/// An <see cref="IActivity"/> to generate the server configuration for an <see cref="Instance"/>.
/// </summary>
public class InstanceServerConfigActivity : ActivityBase<InstanceServerConfigActivity.Inputs, InstanceServerConfigActivity.Outputs>
{
    private readonly ILogger<InstanceServerConfigActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly ServerConfigurationProvider _provider;
    private readonly WriteCaddyfileCommandFactory _writeCaddyfileCommandFactory;
    private readonly WireGuardSetCommandFactory _wireGuardSetCommandFactory;

    /// <summary>
    /// DI constructor for <see cref="InstanceServerConfigActivity"/>.
    /// </summary>
    public InstanceServerConfigActivity(
        ILogger<InstanceServerConfigActivity> logger,
        InstanceProvider instances,
        ServerConfigurationProvider provider,
        WriteCaddyfileCommandFactory writeCaddyfileCommandFactory,
        WireGuardSetCommandFactory wireGuardSetCommandFactory)
    {
        _logger = logger;
        _instances = instances;
        _provider = provider;
        _writeCaddyfileCommandFactory = writeCaddyfileCommandFactory;
        _wireGuardSetCommandFactory = wireGuardSetCommandFactory;
    }

    /// <summary>
    /// The inputs for <see cref="InstanceServerConfigActivity"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance to launch.
        /// </summary>
        [Required]
        [NameSchema]
        [Argument]
        public InstanceId Instance { get; set; }
    }

    /// <summary>
    /// The outputs for <see cref="InstanceServerConfigActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }
    }

    /// <inheritdoc/>
    public override async Task<Outputs?> Execute(Inputs inputs)
    {
        Instance? instanceQuery = await _instances.Get(inputs.Instance);
        if (instanceQuery is not Instance instance)
            return Failure(HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        List<KeyValuePair<ServerId, ShellCommand>> commands = new();
        SetWireGuardPeer(instance, commands);
        if (!await WriteCaddyfile(instance, commands))
            return Failure(HttpStatusCode.InternalServerError);
        // Write configuration
        Dictionary<ServerId, ShellCommand[]> dictionary = commands
            .GroupBy(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());
        if (!await _provider.Put(instance.Name, dictionary))
            return Failure(HttpStatusCode.InternalServerError, "Failed to write server configuration.");
        return Success();
    }

    private void SetWireGuardPeer(Instance instance, List<KeyValuePair<ServerId, ShellCommand>> commands)
    {
        foreach (WireGuardClient wg in instance.WireGuard)
        {
            ShellCommand command = _wireGuardSetCommandFactory.Create(wg);
            commands.Add(new(wg.Server, command));
        }
    }

    private async Task<bool> WriteCaddyfile(Instance instance, List<KeyValuePair<ServerId, ShellCommand>> commands)
    {
        if (instance.Domains.Length == 0)
            return true;
        ShellCommand[] results = await _writeCaddyfileCommandFactory.Create(instance);
        if (results.Length == 0)
            return false;
        commands.AddRange(results
            .Select(x => new KeyValuePair<ServerId, ShellCommand>(instance.Server, x)));
        return true;
    }

    private Outputs Success()
    {
        return new()
        {
            Status = new(HttpStatusCode.Created)
        };
    }

    private Outputs Failure(HttpStatusCode statusCode, string error = "")
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode)
        };
    }
}
