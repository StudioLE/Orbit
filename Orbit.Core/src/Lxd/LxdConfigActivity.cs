using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orbit.Instances;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Lxd;

/// <summary>
/// An <see cref="IActivity"/> to generate the LXD configuration yaml for a virtual machine instance.
/// </summary>
public class LxdConfigActivity : IActivity<LxdConfigActivity.Inputs, string>
{
    private readonly ILogger<LxdConfigActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly LxdConfigProvider _lxdConfigProvider;
    private readonly CommandContext _context;
    private readonly LxdConfigFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigActivity"/>.
    /// </summary>
    public LxdConfigActivity(
        ILogger<LxdConfigActivity> logger,
        InstanceProvider instances,
        LxdConfigProvider lxdConfigProvider,
        CommandContext context,
        LxdConfigFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _lxdConfigProvider = lxdConfigProvider;
        _context = context;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="LxdConfigActivity"/>.
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
        Instance? instanceQuery = _instances.Get(inputs.Instance);
        if (instanceQuery is not Instance instance)
            return Failure("The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure();
        string output = _factory.Create(instance);
        // TODO: Make save optional
        if (!_lxdConfigProvider.Put(instance.Name, output))
            return Failure("Failed to write the lxd config file.");
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
