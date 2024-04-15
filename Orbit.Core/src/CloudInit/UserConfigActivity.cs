using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.Instances;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.CloudInit;

/// <summary>
/// An <see cref="IActivity"/> to generate the cloud init user-data yaml for a virtual machine instance.
/// </summary>
public class UserConfigActivity : IActivity<UserConfigActivity.Inputs, string>
{
    private readonly ILogger<UserConfigActivity> _logger;
    private readonly CloudInitOptions _options;
    private readonly InstanceProvider _instances;
    private readonly UserConfigProvider _provider;
    private readonly CommandContext _context;
    private readonly UserConfigFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="UserConfigActivity"/>.
    /// </summary>
    public UserConfigActivity(
        ILogger<UserConfigActivity> logger,
        IOptions<CloudInitOptions> options,
        InstanceProvider instances,
        UserConfigProvider provider,
        CommandContext context,
        UserConfigFactory factory)
    {
        _logger = logger;
        _options = options.Value;
        _instances = instances;
        _provider = provider;
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="UserConfigActivity"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance to launch.
        /// </summary>
        [Required]
        [NameSchema]
        [Argument]
        public InstanceId Instance { get; set; }
    }

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        if (!_options.TryValidate(_logger))
            return Failure();
        Instance? result = _instances.Get(inputs.Instance);
        if (result is not Instance instance)
            return Failure("The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure();
        string output = _factory.Create(instance);
        // TODO: Make save optional
        if (!_provider.Put(instance.Name, output))
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
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(error);
    }
}
