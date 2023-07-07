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
        Instance instance = new();
        Func<bool>[] steps =
        {
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => WireGuardSetOnServer(instance),
            () => CreateMountsOnServer(instance),
            () => LaunchOnServer(instance)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Launched instance {instance.Name}");
            return Task.FromResult(new Outputs { ExitCode = 0 });
        }
        _logger.LogError("Failed to launch instance.");
        return Task.FromResult(new Outputs { ExitCode = 1 });
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

    private bool WireGuardSetOnServer(Instance instance)
    {
        Network? network = _networks.Get(new NetworkId(instance.Network));
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
        string? cloudInit = _instances.GetResource(new InstanceId(instance.Name), "user-config.yml");
        if (cloudInit is null)
        {
            _logger.LogError("Failed to get user-config");
            return false;
        }
        using SshClient ssh = new(connection);
        ssh.Connect();
        string command = $"sudo wg set wg0 peer {instance.WireGuard.PublicKey} allowed-ips {instance.WireGuard.Addresses.Join(",")}";
        if (ssh.ExecuteToLogger(_logger, command))
            return true;
        _logger.LogError("Failed to add WireGuard peer to server.");
        return false;
    }

    private bool CreateMountsOnServer(Instance instance)
    {
        if (!instance.Mounts.Any())
            return true;
        Server? server = _servers.Get(new ServerId(instance.Server));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        foreach (Mount mount in instance.Mounts)
        {
            if(!mount.Source.StartsWith("/mnt"))
            {
                _logger.LogError($"Mount source path is invalid: {mount.Source}");
                return false;
            }
            string command = $"mkdir -p {mount.Source}";
            if (!ssh.ExecuteToLogger(_logger, command))
            {
                _logger.LogError("Failed to create mount on server.");
                return false;
            }
        }
        return true;
    }

    private bool LaunchOnServer(Instance instance)
    {
        Server? server = _servers.Get(new ServerId(instance.Server));
        if (server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        ConnectionInfo connection = server.CreateConnection();
        string? cloudInit = _instances.GetResource(new InstanceId(instance.Name), "user-config.yml");
        if (cloudInit is null)
        {
            _logger.LogError("Failed to get user-config");
            return false;
        }
        using SshClient ssh = new(connection);
        ssh.Connect();
        string mounts = instance
            .Mounts
            .Select(mount => $@"--mount {mount.Source}:{mount.Target}")
            .Join(@" \" + Environment.NewLine);
        string command = $"""
            (
            cat <<EOF
            {cloudInit}
            EOF
            ) | multipass launch \
                --cpus {instance.Hardware.Cpus} \
                --memory {instance.Hardware.Memory}G \
                --disk {instance.Hardware.Disk}G \
                --name {instance.Name} \
                {mounts} \
                --cloud-init -
            """;
        if (ssh.ExecuteToLogger(_logger, command))
            return true;
        _logger.LogError("Failed to run multipass launch on server.");
        return false;
    }
}
