using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the Caddyfile for a virtual machine instance.
/// </summary>
public class GenerateCaddyfile : IActivity<GenerateCaddyfile.Inputs, string>
{
    public const string FileName = "Caddyfile";
    private readonly ILogger<GenerateCaddyfile> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly CommandContext _context;
    private readonly CaddyfileFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="GenerateCaddyfile"/>.
    /// </summary>
    public GenerateCaddyfile(
        ILogger<GenerateCaddyfile> logger,
        IEntityProvider<Instance> instances,
        CommandContext context,
        CaddyfileFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateCaddyfile"/>.
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
            () => CreateCaddyfile(instance, out output)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Generated Caddyfile for instance {instance.Name}");
            return Task.FromResult(output);
        }
        _logger.LogError("Failed to generate Caddyfile.");
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

    private bool CreateCaddyfile(Instance instance, out string output)
    {
        string? result = _factory.Create(instance);
        if (result is null)
        {
            output = string.Empty;
            return false;
        }
        output = result;
        // TODO: Make save optional
        bool isWritten = _instances.PutResource(new InstanceId(instance.Name), FileName, output);
        if (!isWritten)
            _logger.LogError("Failed to write the Caddyfile.");
        return isWritten;
    }
}
