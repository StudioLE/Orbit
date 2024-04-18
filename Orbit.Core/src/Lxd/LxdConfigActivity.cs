using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Instances;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Tectonic;
using Tectonic.Assets;

namespace Orbit.Lxd;

/// <summary>
/// An <see cref="IActivity"/> to generate the LXD configuration yaml for a virtual machine instance.
/// </summary>
public class LxdConfigActivity : IActivity<LxdConfigActivity.Inputs, LxdConfigActivity.Outputs>
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
        public InternalAsset? Asset { get; set; }
    }

    /// <inheritdoc/>
    public async Task<Outputs> Execute(Inputs inputs)
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
            Asset = new()
            {
                ContentType = "text/x-yaml",
                Content = output
            }
        };
    }

    private Outputs Failure(HttpStatusCode statusCode, string? error = null, string output = "")
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode),
            Asset = new()
            {
                ContentType = "text/x-yaml",
                Content = output
            }
        };
    }
}
