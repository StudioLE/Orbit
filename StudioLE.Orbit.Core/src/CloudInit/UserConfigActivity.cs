using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudioLE.Orbit.Assets;
using StudioLE.Orbit.Instances;
using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Schema.DataAnnotations;
using StudioLE.Orbit.Utils.DataAnnotations;
using Tectonic;
using FileInfo = StudioLE.Storage.Files.FileInfo;

namespace StudioLE.Orbit.CloudInit;

/// <summary>
/// An <see cref="IActivity"/> to generate the cloud init user-data yaml for a virtual machine instance.
/// </summary>
public class UserConfigActivity : ActivityBase<UserConfigActivity.Inputs, UserConfigActivity.Outputs>
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
        public FileInfo? Asset { get; set; }
    }

    /// <inheritdoc/>
    public override async Task<Outputs?> Execute(Inputs inputs)
    {
        if (!_options.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        Instance? result = await _instances.Get(inputs.Instance);
        if (result is not Instance instance)
            return Failure(HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        string output = await _factory.Create(instance);
        // TODO: Make save optional
        if (!await _provider.Put(instance.Name, output))
            return Failure(HttpStatusCode.InternalServerError, "Failed to save the user config.");
        return Success(output);
    }

    private Outputs Success(string output)
    {
        return new()
        {
            Status = new(HttpStatusCode.OK),
            Asset = AssetHelpers.CreateFromYaml(output)
        };
    }

    private Outputs Failure(HttpStatusCode statusCode, string? error = null, string output = "")
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode),
            Asset = AssetHelpers.CreateFromYaml(output)
        };
    }
}
