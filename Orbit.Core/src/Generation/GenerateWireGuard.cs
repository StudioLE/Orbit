using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the WireGuard configuration file for a virtual machine instance.
/// </summary>
public class GenerateWireGuard : IActivity<GenerateWireGuard.Inputs, string>
{
    private readonly ILogger<GenerateWireGuard> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly CommandContext _context;
    private readonly WireGuardConfigFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="GenerateWireGuard"/>.
    /// </summary>
    public GenerateWireGuard(
        ILogger<GenerateWireGuard> logger,
        IEntityProvider<Instance> instances,
        CommandContext context,
        WireGuardConfigFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateWireGuard"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance.
        /// </summary>
        [Required]
        [NameSchema]
        public string Instance { get; set; } = string.Empty;

        /// <summary>
        /// The name of the wireguard interface to generate.
        /// </summary>
        [Required]
        [NameSchema]
        public string Interface { get; set; } = string.Empty;
    }

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        Instance instance = new();
        string output = string.Empty;
        WireGuard wg = null!;
        Func<bool>[] steps =
        {
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => GetWireGuard(instance, inputs.Interface, out wg),
            () => CreateWireGuardConfig(instance, wg, out output)
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

    private bool GetWireGuard(Instance instance, string interfaceName, out WireGuard wg)
    {
        WireGuard? result = instance
            .WireGuard
            .FirstOrDefault(x => x.Name == interfaceName);
        wg = result!;
        if (result is null)
        {
            _logger.LogError("The interface does not exist.");
            return false;
        }
        return true;
    }

    private bool CreateWireGuardConfig(Instance instance, WireGuard wg, out string output)
    {
        output = _factory.Create(wg);
        // TODO: Make save optional
        string fileName = wg.GetConfigFileName();
        if (_instances.PutResource(new InstanceId(instance.Name), fileName, output))
            return true;
        _logger.LogError("Failed to write the WireGuard config file.");
        return false;
    }
}
