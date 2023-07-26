using Orbit.Core.Schema;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Generation;

public class CloneRepoCommandFactory : IFactory<Instance, string?>
{
    /// <inheritdoc/>
    public string? Create(Instance instance)
    {
        return instance.Repo is null
            ? null
            : $"""
            git clone {instance.Repo.Origin} /mnt/{instance.Name}/srv --branch {instance.Repo.Branch} --depth 1
            """;
    }
}
