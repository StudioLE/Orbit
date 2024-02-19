using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Orbit.Utils.Shell;

/// <summary>
/// Execute a shell command on a remote server via SSH - with optional standard input - and capture the output.
/// </summary>
/// <remarks>
/// <inheritdoc/>
/// </remarks>
public class SecureShellCommand : ShellCommand
{
    public SecureShellOptions? Options { get; set; } = null;

    /// <summary>
    /// Creates a new instance of <see cref="SecureShellCommand"/>.
    /// </summary>
    public SecureShellCommand()
    {
        ConfigureStartInfo = ConfigureStartInfoForSsh;
    }

    /// <summary>
    /// DI constructor for <see cref="SecureShellCommand"/>.
    /// </summary>
    public SecureShellCommand(ILogger<SecureShellCommand> logger) : this()
    {
        Logger = logger;
    }

    private void ConfigureStartInfoForSsh(ProcessStartInfo startInfo)
    {
        if (Options is null)
            throw new("Options must be set.");
        startInfo.FileName = "ssh";
        StringBuilder args = new();
        args.Append(" " + Options.Host);
        args.Append(" -l " + Options.User);
        args.Append(" -p " + Options.Port);
        args.Append(" -o BatchMode=yes");
        args.Append(" -o StrictHostKeyChecking=no");
        args.Append(" -o UserKnownHostsFile=/dev/null");
        args.Append(" " + startInfo.FileName + " " + startInfo.Arguments);
        startInfo.Arguments = args.ToString();
    }
}
