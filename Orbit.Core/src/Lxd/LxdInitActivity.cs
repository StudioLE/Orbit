using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Instances;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Servers;
using Orbit.Utils.CommandLine;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Lxd;

/// <summary>
/// An <see cref="IActivity"/> to initialize an <see cref="Instance"/> on a <see cref="Server"/>
/// using LXD.
/// </summary>
public class LxdInitActivity : IActivity<LxdInitActivity.Inputs, LxdInitActivity.Outputs>
{
    private readonly ILogger<LxdInitActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly ServerProvider _servers;
    private readonly LxdConfigProvider _lxdConfigProvider;
    private readonly LxdConfigFactory _lxdConfigFactory;
    private readonly Ssh _ssh;

    /// <summary>
    /// DI constructor for <see cref="LxdInitActivity"/>.
    /// </summary>
    public LxdInitActivity(
        ILogger<LxdInitActivity> logger,
        InstanceProvider instances,
        ServerProvider servers,
        LxdConfigProvider lxdConfigProvider,
        LxdConfigFactory lxdConfigFactory,
        Ssh ssh)
    {
        _logger = logger;
        _instances = instances;
        _servers = servers;
        _lxdConfigProvider = lxdConfigProvider;
        _lxdConfigFactory = lxdConfigFactory;
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

    /// <summary>
    /// The outputs for <see cref="LxdInitActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }
    }

    /// <inheritdoc/>
    public async Task<Outputs> Execute(Inputs inputs)
    {
        Instance? instanceQuery = await _instances.Get(new(inputs.Instance));
        if (instanceQuery is not Instance instance)
            return Failure(HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        Server? serverQuery = await _servers.Get(instance.Server);
        if (serverQuery is not Server server)
            return Failure(HttpStatusCode.NotFound, "The server does not exist");
        _ssh.SetServer(server);
        string? config = await _lxdConfigProvider.Get(instance.Name);
        if (config is not null)
            _logger.LogDebug("Using existing LXD config.");
        else
        {
            _logger.LogDebug("Generating LXD config for instance.");
            config = await _lxdConfigFactory.Create(instance);
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
            return Failure(HttpStatusCode.InternalServerError, "Failed to run LXD init on server.");
        _logger.LogInformation($"Initialized instance {instance.Name} on server {server.Name}.");
        return Success();
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
