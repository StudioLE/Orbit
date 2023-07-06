using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils.DataAnnotations;
using Renci.SshNet;
using StudioLE.Core.System;

namespace Orbit.Core.Activities;

/// <summary>
/// An <see cref="IActivity"/> to remotely launch an instance with Multipass.
/// </summary>
public class Launch : IActivity<Launch.Inputs, Launch.Outputs>
{
    private readonly ILogger<Launch> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IEntityProvider<Network> _networks;
    private readonly IEntityProvider<Server> _servers;
    private Instance _instance = null!;

    /// <summary>
    /// DI constructor for <see cref="Launch"/>.
    /// </summary>
    public Launch(ILogger<Launch> logger, IEntityProvider<Instance> instances, IEntityProvider<Network> networks, IEntityProvider<Server> servers)
    {
        _logger = logger;
        _instances = instances;
        _networks = networks;
        _servers = servers;
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

    /// <summary>
    /// The outputs of <see cref="Launch"/>.
    /// </summary>
    public class Outputs
    {
        public int ExitCode { get; set; }
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
    {
        Func<bool>[] steps =
        {
            () => GetInstance(inputs.Instance),
            () => _instance.TryValidate(_logger),
            WireGuardSetOnServer,
            LaunchOnServer
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Launched instance {_instance.Name}");
            return Task.FromResult(new Outputs { ExitCode = 0 });
        }
        _logger.LogError("Failed to launch instance.");
        return Task.FromResult(new Outputs { ExitCode = 1 });
    }

    private bool GetInstance(string instanceName)
    {
        Instance? instance = _instances.Get(new InstanceId(instanceName));
        if (instance is null)
        {
            _logger.LogError("The instance does not exist.");
            return false;
        }
        _instance = instance;
        return true;
    }

    private bool WireGuardSetOnServer()
    {
        Network? network = _networks.Get(new NetworkId(_instance.Network));
        if (network is null)
        {
            _logger.LogError("Failed to get network");
            return false;
        }
        Server? server = _servers.Get(new ServerId(network.Server));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        string? cloudInit = _instances.GetResource(new InstanceId(_instance.Name), "user-config.yml");
        if (cloudInit is null)
        {
            _logger.LogError("Failed to get user-config");
            return false;
        }
        using SshClient ssh = new(connection);
        ssh.Connect();
        string command = $"sudo wg set wg0 peer {_instance.WireGuard.PublicKey} allowed-ips {_instance.WireGuard.Addresses.Join(",")}";
        if (ssh.ExecuteToLogger(_logger, command))
            return true;
        _logger.LogError("Failed to launch instance");
        return false;
    }

    private bool LaunchOnServer()
    {
        Server? server = _servers.Get(new ServerId(_instance.Server));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        string? cloudInit = _instances.GetResource(new InstanceId(_instance.Name), "user-config.yml");
        if (cloudInit is null)
        {
            _logger.LogError("Failed to get user-config");
            return false;
        }
        using SshClient ssh = new(connection);
        ssh.Connect();
        string command = $"""
            (
            cat <<EOF
            {cloudInit}
            EOF
            ) | multipass launch \
                --cpus {_instance.Hardware.Cpus} \
                --memory {_instance.Hardware.Memory}G \
                --disk {_instance.Hardware.Disk}G \
                --name {_instance.Name} \
                --cloud-init -
            """;
        if (ssh.ExecuteToLogger(_logger, command))
            return true;
        _logger.LogError("Failed to launch instance");
        return false;
    }
}
