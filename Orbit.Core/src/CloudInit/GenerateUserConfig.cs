using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.CloudInit;

/// <summary>
/// An <see cref="IActivity"/> to generate the cloud init user-data yaml for a virtual machine instance.
/// </summary>
public class GenerateUserConfig : IActivity<GenerateUserConfig.Inputs, string>
{
    public const string FileName = "user-config.yml";
    private readonly ILogger<GenerateUserConfig> _logger;
    private readonly CloudInitOptions _options;
    private readonly IEntityProvider<Instance> _instances;
    private readonly CommandContext _context;
    private readonly UserConfigFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="GenerateUserConfig"/>.
    /// </summary>
    public GenerateUserConfig(
        ILogger<GenerateUserConfig> logger,
        IOptions<CloudInitOptions> options,
        IEntityProvider<Instance> instances,
        CommandContext context,
        UserConfigFactory factory)
    {
        _logger = logger;
        _options = options.Value;
        _instances = instances;
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateUserConfig"/>.
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
        if(!_options.TryValidate(_logger))
            return Failure();
        Instance? instance = _instances.Get(new InstanceId(inputs.Instance));
        if (instance is null)
            return Failure("The instance does not exist.");
        if(!instance.TryValidate(_logger))
            return Failure();
        string output = _factory.Create(instance);
        // TODO: Make save optional
        if (!_instances.PutResource(new InstanceId(instance.Name), FileName, output))
            return Failure("Failed to write the user config file.");
        return Success(string.Empty);
    }

    private Task<string> Success(string output)
    {
        _context.ExitCode = 0;
        return Task.FromResult(output);
    }

    private Task<string> Failure(string error = "", int exitCode = 1)
    {
        if(!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(error);
    }
}
