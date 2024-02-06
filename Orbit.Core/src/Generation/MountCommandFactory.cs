using Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Generation;

public class MountCommandFactory : IFactory<Instance, string>
{
    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        return instance
            .Mounts
            .Select(mount => Create(instance, mount))
            .Join(Environment.NewLine + Environment.NewLine);
    }

    private static string Create(Instance instance, Mount mount)
    {
        return $"""
            mkdir -p {mount.Source}
            multipass mount {mount.Source} {instance.Name}:{mount.Target}
            """;
    }
}
