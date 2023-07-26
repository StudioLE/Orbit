using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Creation;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine instance.
/// </summary>
public class CreateInstance : IActivity<Instance, Instance?>
{
    private readonly ILogger<CreateInstance> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly InstanceFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="CreateInstance"/>.
    /// </summary>
    public CreateInstance(ILogger<CreateInstance> logger, IEntityProvider<Instance> instances, InstanceFactory factory)
    {
        _logger = logger;
        _instances = instances;
        _factory = factory;
    }

    /// <inheritdoc/>
    public Task<Instance?> Execute(Instance instance)
    {
        Func<bool>[] steps =
        {
            () => UpdateInstanceProperties(ref instance),
            () => ValidateInstance(instance),
            () => PutInstance(instance)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Created instance {instance.Name}");
            return Task.FromResult<Instance?>(instance);
        }
        _logger.LogError("Failed to create instance.");
        return Task.FromResult<Instance?>(null);
    }

    private bool UpdateInstanceProperties(ref Instance instance)
    {
        instance = _factory.Create(instance);
        return true;
    }

    private bool ValidateInstance(Instance instance)
    {
        return instance.TryValidate(_logger);
    }

    private bool PutInstance(Instance instance)
    {
        if (_instances.Put(instance))
            return true;
        _logger.LogError("Failed to write the instance file.");
        return false;
    }
}
