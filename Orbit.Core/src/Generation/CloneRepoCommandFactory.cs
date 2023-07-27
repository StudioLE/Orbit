using Orbit.Core.Schema;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Generation;

public class CloneRepoCommandFactory : IFactory<Instance, ShellCommand?>
{
    /// <inheritdoc/>
    public ShellCommand? Create(Instance instance)
    {
        return instance.Repo is null
            ? null
            : new ShellCommand
            {
                Command = $"""
                    git clone {instance.Repo.Origin} /mnt/{instance.Name}/srv --branch {instance.Repo.Branch} --depth 1
                    """,
                ErrorMessage = $"Failed to clone repository: {instance.Repo.Origin}"
            };
    }
}
