using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Creation.Instances;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine instance.
/// </summary>
public class CreateInstance : IActivity<Instance, Instance>
{
    private readonly ILogger<CreateInstance> _logger;
    private readonly IEntityProvider<Instance> _instances;
    private readonly InstanceFactory _factory;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="CreateInstance"/>.
    /// </summary>
    public CreateInstance(
        ILogger<CreateInstance> logger,
        IEntityProvider<Instance> instances,
        InstanceFactory factory,
        CommandContext context)
    {
        _logger = logger;
        _instances = instances;
        _factory = factory;
        _context = context;
    }

    /// <inheritdoc/>
    public Task<Instance> Execute(Instance instance)
    {
        instance = _factory.Create(instance);
        if (!instance.TryValidate(_logger))
            return Failure(instance);
        if (!_instances.Put(instance))
            return Failure(instance, "Failed to write the instance file.");
        _logger.LogInformation($"Created instance {instance.Name}");
        return Success(instance);
    }

    private Task<Instance> Success(Instance instance)
    {
        _context.ExitCode = 0;
        return Task.FromResult(instance);
    }

    private Task<Instance> Failure(Instance instance, string error = "", int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(instance);
    }
}
