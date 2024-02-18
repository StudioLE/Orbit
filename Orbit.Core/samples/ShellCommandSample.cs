using Microsoft.Extensions.Logging;
using Orbit.Shell;

namespace Orbit.Core.Shell.Sample;

public class ShellCommandSample : ShellCommand
{

    /// <summary>
    /// DI constructor for <see cref="ShellCommandSample"/>.
    /// </summary>
    public ShellCommandSample(ILogger<ShellCommandSample> logger) : base(logger)
    {

    }

    public Task<int> Execute()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "countdown");
        if (!Path.Exists(path))
            throw new("The countdown executable is missing.");
        return Execute(path, "5 0.5 0");
    }
}
