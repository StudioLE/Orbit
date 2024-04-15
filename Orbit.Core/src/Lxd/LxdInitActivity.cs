using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orbit.Instances;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.CommandLine;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Lxd;

/// <summary>
/// An <see cref="IActivity"/> to initialize an <see cref="Instance"/> on a <see cref="Server"/>
/// using LXD.
/// </summary>
public class LxdInitActivity : IActivity<LxdInitActivity.Inputs, string>
{
    private readonly ILogger<LxdInitActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly IEntityProvider<Server> _servers;
    private readonly LxdConfigProvider _lxdConfigProvider;
    private readonly LxdConfigFactory _lxdConfigFactory;
    private readonly CommandContext _context;
    private readonly Ssh _ssh;

    /// <summary>
    /// DI constructor for <see cref="LxdInitActivity"/>.
    /// </summary>
    public LxdInitActivity(
        ILogger<LxdInitActivity> logger,
        InstanceProvider instances,
        IEntityProvider<Server> servers,
        LxdConfigProvider lxdConfigProvider,
        LxdConfigFactory lxdConfigFactory,
        CommandContext context,
        Ssh ssh)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _lxdConfigProvider = lxdConfigProvider;
        _lxdConfigFactory = lxdConfigFactory;
        _context = context;
        _ssh = ssh;
    }

    /// <summary>
    /// The inputs for <see cref="LxdInitActivity"/>.
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
            return Failure("The server does not exist");
        _ssh.SetServer(server);
        string? config = _lxdConfigProvider.Get(instance.Name);
        if (config is not null)
            _logger.LogDebug("Using existing LXD config.");
        else
        {
            _logger.LogDebug("Generating LXD config for instance.");
            config = _lxdConfigFactory.Create(instance);
        }
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
        _logger.LogInformation($"Initialized instance {instance.Name} on server {server.Name}.");
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
