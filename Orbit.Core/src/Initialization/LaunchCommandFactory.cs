using Microsoft.Extensions.Logging;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Initialization;

public class LaunchCommandFactory : IFactory<Instance, string?>
{
    private readonly ILogger<LaunchCommandFactory> _logger;
    private readonly IEntityProvider<Instance> _instances;

    public LaunchCommandFactory(ILogger<LaunchCommandFactory> logger, IEntityProvider<Instance> instances)
    {
        _instances = instances;
        _logger = logger;
    }

    /// <inheritdoc/>
    public string? Create(Instance instance)
    {
        string? cloudInit = _instances.GetResource(new InstanceId(instance.Name), GenerateInstanceConfiguration.FileName);
        if (cloudInit is null)
        {
            _logger.LogError("Failed to get user-config");
            return null;
        }
        return $"""
            (
            cat <<EOF
            {cloudInit}
            EOF
            ) | multipass launch \
                --cpus {instance.Hardware.Cpus} \
                --memory {instance.Hardware.Memory}G \
                --disk {instance.Hardware.Disk}G \
                --name {instance.Name} \
                --cloud-init -
            """;
    }
}
