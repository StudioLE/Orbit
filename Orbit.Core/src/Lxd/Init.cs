using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.CommandLine;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Lxd;

/// <summary>
/// An <see cref="IActivity"/> to remotely launch an instance with Multipass.
/// </summary>
public class Init : IActivity<Init.Inputs, string>
{
    private readonly ILogger<Init> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly CommandContext _context;
    private readonly Ssh _ssh;

    /// <summary>
    /// DI constructor for <see cref="Init"/>.
    /// </summary>
    public Init(
        ILogger<Init> logger,
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        CommandContext context,
        Ssh ssh)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _context = context;
        _ssh = ssh;
    }

    /// <summary>
    /// The inputs for <see cref="Init"/>.
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
        Server? serverQuery = _servers.Get(instance.Server);
        if (serverQuery is not Server server)
            return Failure("Failed to get server");
        _ssh.SetServer(server);
        string? config = _instances.GetArtifact(instance.Name, GenerateLxdConfig.FileName);
        if (config is null)
            return Failure("Failed to get user-config");
        string[] args =
        [
            "lxc",
            "init",
            $"{instance.OS.Name.ToLower()}:{instance.OS.Version}",
            instance.Name.ToString(),
            "--vm"
        ];
        int exitCode = _ssh.Execute(string.Join(" ", args), config);
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
