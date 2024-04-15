using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.Instances;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine instance.
/// </summary>
public class InstanceActivity : IActivity<Instance, InstanceActivity.Outputs>
{
    private readonly ILogger<InstanceActivity> _logger;
    private readonly InstanceProvider _instances;
    private readonly InstanceFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="InstanceActivity"/>.
    /// </summary>
    public InstanceActivity(
        ILogger<InstanceActivity> logger,
        InstanceProvider instances,
        InstanceFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _factory = factory;
    }

    /// <summary>
    /// The outputs for <see cref="InstanceActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The client.
        /// </summary>
        public Instance Instance { get; set; }
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Instance instance)
    {
        instance = _factory.Create(instance);
        if (!instance.TryValidate(_logger))
            return Failure(instance, HttpStatusCode.BadRequest);
        if (!_instances.Put(instance))
            return Failure(instance, HttpStatusCode.InternalServerError, "Failed to write the instance file.");
        _logger.LogInformation($"Created instance {instance.Name}");
        return Success(instance);
    }


    private Task<Outputs> Success(Instance instance)
    {
        Outputs outputs = new()
        {
            Status = new(HttpStatusCode.Created),
            Instance = instance
        };
        return Task.FromResult(outputs);
    }

    private Task<Outputs> Failure(Instance instance, HttpStatusCode statusCode, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        Outputs outputs = new()
        {
            Status = new(statusCode),
            Instance = instance
        };
        return Task.FromResult(outputs);
    }
}
