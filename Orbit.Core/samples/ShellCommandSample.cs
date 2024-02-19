using Microsoft.Extensions.Logging;
using Orbit.Utils.Shell;

namespace Orbit.Core.Shell.Sample;

public class ShellCommandSample
{
    private readonly ILogger<ShellCommandSample> _logger;

    /// <summary>
    /// DI constructor for <see cref="ShellCommandSample"/>.
    /// </summary>
    public ShellCommandSample(ILogger<ShellCommandSample> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Execute the shell command.
    /// </summary>
    public int Execute()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "countdown");
        if (!Path.Exists(path))
            throw new("The countdown executable is missing.");
        ShellCommand cmd = new()
        {
            Logger = _logger,
            TimeOut = 10_000
        };
        return cmd.Execute(path, "2 0.5 0");
    }
}
