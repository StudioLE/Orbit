using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.Instances;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Tectonic;
using Tectonic.Assets;

namespace Orbit.CloudInit;

/// <summary>
/// An <see cref="IActivity"/> to generate the cloud init user-data yaml for a virtual machine instance.
/// </summary>
public class UserConfigActivity : IActivity<UserConfigActivity.Inputs, UserConfigActivity.Outputs>
{
    private readonly ILogger<UserConfigActivity> _logger;
    private readonly CloudInitOptions _options;
    private readonly InstanceProvider _instances;
    private readonly UserConfigProvider _provider;
    private readonly UserConfigFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="UserConfigActivity"/>.
    /// </summary>
    public UserConfigActivity(
        ILogger<UserConfigActivity> logger,
        IOptions<CloudInitOptions> options,
        InstanceProvider instances,
        UserConfigProvider provider,
        UserConfigFactory factory)
    {
        _logger = logger;
        _options = options.Value;
        _instances = instances;
        _provider = provider;
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

    /// <summary>
    /// The outputs for <see cref="UserConfigActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The generated asset.
        /// </summary>
        public InternalAsset? Asset { get; set; }
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
    {
        if (!_options.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        Instance? result = _instances.Get(inputs.Instance);
        if (result is not Instance instance)
            return Failure(HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        string output = _factory.Create(instance);
        // TODO: Make save optional
        if (!_provider.Put(instance.Name, output))
            return Failure(HttpStatusCode.InternalServerError, "Failed to save the user config.");
        return Success(output);
    }

    private Task<Outputs> Success(string output)
    {
        Outputs outputs = new()
        {
            Status = new(HttpStatusCode.OK),
            Asset = new()
            {
                ContentType = "text/x-yaml",
                Content = output
            }
        };
        return Task.FromResult(outputs);
    }

    private Task<Outputs> Failure(HttpStatusCode statusCode, string? error = null, string output = "")
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        Outputs outputs = new()
        {
            Status = new(statusCode),
            Asset = new()
            {
                ContentType = "text/x-yaml",
                Content = output
            }
        };
        return Task.FromResult(outputs);
    }
}
