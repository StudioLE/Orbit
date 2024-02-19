using Orbit.Schema;
using StudioLE.Patterns;

namespace Orbit.Generation;

public class CloneRepoCommandFactory : IFactory<Instance, PreparedShellCommand?>
{
    /// <inheritdoc/>
    public PreparedShellCommand? Create(Instance instance)
    {
        return instance.Repo is null
            ? null
            : new PreparedShellCommand
            {
                Command = $"""
                    git clone {instance.Repo.Origin} /mnt/{instance.Name}/srv --branch {instance.Repo.Branch} --depth 1
                    """,
                ErrorMessage = $"Failed to clone repository: {instance.Repo.Origin}"
            };
    }
}
