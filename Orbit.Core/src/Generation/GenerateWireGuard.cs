using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the WireGuard configuration file for a virtual machine instance.
/// </summary>
public class GenerateWireGuard : IActivity<GenerateWireGuard.Inputs, string>
{
    public const string FileName = "wg0.conf";
    private readonly ILogger<GenerateWireGuard> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="GenerateWireGuard"/>.
    /// </summary>
    public GenerateWireGuard(
        ILogger<GenerateWireGuard> logger,
        IEntityProvider<Instance> instances,
        CommandContext context)
    {
        _logger = logger;
        _instances = instances;
        _context = context;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateWireGuard"/>.
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

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        Instance instance = new();
        string output = string.Empty;
        Func<bool>[] steps =
        {
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => CreateWireGuardConfig(instance, out output)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Generated WireGuard config for instance {instance.Name}");
            return Task.FromResult(output);
        }
        _logger.LogError("Failed to generate WireGuard config.");
        _context.ExitCode = 1;
        return Task.FromResult(output);
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

    private bool CreateWireGuardConfig(Instance instance, out string output)
    {
        List<string> lines = new()
        {
            $"""
            [Interface]
            PrivateKey = {instance.WireGuard.PrivateKey}
            """
        };
        foreach (string address in instance.WireGuard.Addresses)
            lines.Add($"Address = {address}");
        lines.Add($"""
            [Peer]
            PublicKey = {instance.WireGuard.ServerPublicKey}
            AllowedIPs = {instance.WireGuard.AllowedIPs.Join(", ")}
            Endpoint = {instance.WireGuard.Endpoint}
            """);
        output = lines.Join();
        if (_instances.PutResource(new InstanceId(instance.Name), FileName, output))
            return true;
        _logger.LogError("Failed to write the WireGuard config file.");
        return false;
    }
}
