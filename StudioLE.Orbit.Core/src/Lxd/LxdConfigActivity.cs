using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using StudioLE.Orbit.Assets;
using StudioLE.Orbit.Instances;
using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Schema.DataAnnotations;
using StudioLE.Orbit.Utils.DataAnnotations;
using Tectonic;
using FileInfo = StudioLE.Storage.Files.FileInfo;

namespace StudioLE.Orbit.Lxd;

/// <summary>
/// An <see cref="IActivity"/> to generate the LXD configuration yaml for a virtual machine instance.
/// </summary>
public class LxdConfigActivity : ActivityBase<LxdConfigActivity.Inputs, LxdConfigActivity.Outputs>
{
    private readonly ILogger<LxdConfigActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly LxdConfigProvider _lxdConfigProvider;
    private readonly LxdConfigFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigActivity"/>.
    /// </summary>
    public LxdConfigActivity(
        ILogger<LxdConfigActivity> logger,
        InstanceProvider instances,
        LxdConfigProvider lxdConfigProvider,
        LxdConfigFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _lxdConfigProvider = lxdConfigProvider;
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

    /// <summary>
    /// The outputs for <see cref="LxdConfigActivity"/>.
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
        Instance? instanceQuery = await _instances.Get(inputs.Instance);
        if (instanceQuery is not Instance instance)
            return Failure(HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
        string output = await _factory.Create(instance);
        // TODO: Make save optional
        if (!await _lxdConfigProvider.Put(instance.Name, output))
            return Failure(HttpStatusCode.InternalServerError, "Failed to write the lxd config file.");
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
