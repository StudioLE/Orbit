using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orbit.Utils.Shell;

/// <summary>
/// Execute a shell command on a remote server via SSH - with optional standard input - and capture the output.
/// </summary>
/// <remarks>
/// <inheritdoc/>
/// </remarks>
public class SecureShellCommand : ShellCommand
{
    /// <summary>
    /// The SSH host.
    /// </summary>
    public string? Host { get; set; } = null;

    /// <summary>
    /// The SSH port.
    /// </summary>
    [Range(0, 65536)]
    public int? Port { get; set; } = null;

    /// <summary>
    /// The SSH user.
    /// </summary>
    public string? User { get; set; } = null;

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

    /// <summary>
    /// DI constructor for <see cref="SecureShellCommand"/>.
    /// </summary>
    public SecureShellCommand(ILogger<SecureShellCommand> logger, IOptions<SecureShellOptions> options) : this(logger)
    {
        Host = options.Value.Host;
        Port = options.Value.Port;
        User = options.Value.User;
    }

    private void ConfigureStartInfoForSsh(ProcessStartInfo startInfo)
    {
        if (string.IsNullOrEmpty(Host))
            throw new("Host must be set.");
        StringBuilder args = new();
        args.Append(" " + Host);
        if (Port is not null)
            args.Append(" -p " + Port);
        if (User is not null)
            args.Append(" -l " + User);
        args.Append(" -o BatchMode=yes");
        args.Append(" -o StrictHostKeyChecking=yes");
        args.Append(" " + startInfo.FileName + " " + startInfo.Arguments);
        startInfo.FileName = "ssh";
        startInfo.Arguments = args.ToString();
    }
}
