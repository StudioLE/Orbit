using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Schema.DataAnnotations;
using StudioLE.Orbit.Utils.DataAnnotations;
using Tectonic;

namespace StudioLE.Orbit.Instances;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine instance.
/// </summary>
public class InstanceUpdateActivity : ActivityBase<InstanceUpdateActivity.Inputs, InstanceUpdateActivity.Outputs>
{
    private readonly ILogger<InstanceUpdateActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly InstanceFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="InstanceUpdateActivity"/>.
    /// </summary>
    public InstanceUpdateActivity(
        ILogger<InstanceUpdateActivity> logger,
        InstanceProvider instances,
        InstanceFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _factory = factory;
    }

    /// <summary>
    /// The inputs for <see cref="InstanceUpdateActivity"/>.
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
    /// The outputs for <see cref="InstanceUpdateActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The instance.
        /// </summary>
        public Instance? Instance { get; set; }
    }

    /// <inheritdoc/>
    public override async Task<Outputs?> Execute(Inputs inputs)
    {
        Instance? instanceQuery = await _instances.Get(inputs.Instance);
        if (instanceQuery is not Instance instance)
            return Failure(null, HttpStatusCode.NotFound, "The instance does not exist.");
        if (!instance.TryValidate(_logger))
            return Failure(instance, HttpStatusCode.BadRequest);
        if (!await _instances.Put(instance))
            return Failure(instance, HttpStatusCode.InternalServerError, "Failed to write the instance file.");
        _logger.LogInformation($"Created instance {instance.Name}");
        return Success(instance);
    }

    private Outputs Success(Instance instance)
    {
        return new()
        {
            Status = new(HttpStatusCode.Created),
            Instance = instance
        };
    }

    private Outputs Failure(Instance? instance, HttpStatusCode statusCode, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode),
            Instance = instance
        };
    }
}
