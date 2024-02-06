using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the cloud init user-data yaml for a virtual machine instance.
/// </summary>
public class GenerateInstanceConfiguration : IActivity<GenerateInstanceConfiguration.Inputs, string>
{
    public const string FileName = "user-config.yml";
    private readonly ILogger<GenerateInstanceConfiguration> _logger;
    private readonly CloudInitOptions _options;
    private readonly IEntityProvider<Instance> _instances;
    private readonly CommandContext _context;
    private readonly CloudInitFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="GenerateInstanceConfiguration"/>.
    /// </summary>
    public GenerateInstanceConfiguration(
        ILogger<GenerateInstanceConfiguration> logger,
        IOptions<CloudInitOptions> options,
        IEntityProvider<Instance> instances,
        CommandContext context,
        CloudInitFactory factory)
    {
        _logger = logger;
        _options = options.Value;
        _instances = instances;
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateInstanceConfiguration"/>.
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
            () => ValidateOptions(),
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => CreateUserConfig(instance, out output)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation("Generated instance configuration");
            return Task.FromResult(output);
        }
        _logger.LogError("Failed to generate instance configuration.");
        _context.ExitCode = 1;
        return Task.FromResult(output);
    }

    private bool ValidateOptions()
    {
        return _options.TryValidate(_logger);
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

    private bool CreateUserConfig(Instance instance, out string output)
    {
        output = _factory.Create(instance);
        // TODO: Make save optional
        if (_instances.PutResource(new InstanceId(instance.Name), FileName, output))
            return true;
        _logger.LogError("Failed to write the user config file.");
        return false;
    }
}
