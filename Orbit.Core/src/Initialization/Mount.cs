using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
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
public class Mount : IActivity<Mount.Inputs, string>
{
    private readonly ILogger<Mount> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="Mount"/>.
    /// </summary>
    public Mount(
        ILogger<Mount> logger,
        IEntityProvider<Instance> instances,
        IEntityProvider<Server> servers,
        CommandContext context)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _context = context;
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
        if (!instance.Mounts.Any())
            return Failure("No mounts to create.");
        Server? server = _servers.Get(new ServerId(instance.Server));
        if (server is null)
            return Failure("Failed to get server");
        Ssh ssh = MultipassHelpers.CreateSsh(_logger, server);
        foreach (Schema.Mount mount in instance.Mounts)
        {
            if (!mount.Source.StartsWith("/mnt"))
                return Failure($"Mount source path is invalid: {mount.Source}");
            string mkdirCommand = $"mkdir -p {mount.Source}";
            int exitCode = ssh.Execute(mkdirCommand, string.Empty);
            if (exitCode != 0)
                return Failure("Failed to create mount directory on server.");
            string multipassCommand = $"multipass mount {mount.Source} {instance.Name}:{mount.Target}";
            exitCode = ssh.Execute(multipassCommand, string.Empty);
            if (exitCode != 0)
                return Failure($"Failed to create mount: {instance.Name}:{mount.Target}");
        }
        _logger.LogInformation($"Created mounts for instance {instance.Name}");
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
