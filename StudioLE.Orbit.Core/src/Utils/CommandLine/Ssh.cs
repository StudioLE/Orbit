using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;

namespace StudioLE.Orbit.Utils.CommandLine;

/// <summary>
/// Execute a shell command on a remote server via SSH - with optional standard input - and capture the output.
/// </summary>
/// <remarks>
/// <see cref="Ssh"/> is a wrapper around <see cref="Process"/> that handles the
/// frustrations quirks of the dealing with <see cref="Process"/> directly.
/// </remarks>
public class Ssh
{
    private readonly Cli _cli;

    /// <summary>
    /// The SSH host.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// The SSH port.
    /// </summary>
    [Range(0, 65536)]
    public int? Port { get; set; }

    /// <summary>
    /// The SSH user.
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// DI constructor for <see cref="Ssh"/>.
    /// </summary>
    public Ssh(Cli cli)
    {
        _cli = cli;
        _cli.TimeOut = 12 * 60 * 1000;
    }

    /// <summary>
    /// DI constructor for <see cref="Ssh"/>.
    /// </summary>
    public Ssh(Cli cli, IOptions<SshOptions> options) : this(cli)
    {
        Host = options.Value.Host;
        Port = options.Value.Port;
        User = options.Value.User;
    }

    /// <summary>
    /// Execute a shell command - with optional standard input - and capture the output.
    /// </summary>
    /// <param name="command">The command and arguments to execute.</param>
    /// <param name="standardInput">Optional strings to pass to standard input.</param>
    /// <returns>
    /// The process exit code if the command is executed,
    /// or an integer value of <see cref="Cli.ExitCode"/> if something goes wrong.
    /// </returns>
    public int Execute(string command, string standardInput)
    {
        string args = FormatArguments(command);
        return _cli.Execute("ssh", args, standardInput);
    }

    private string FormatArguments(string command)
    {
        if (string.IsNullOrEmpty(Host))
            throw new("Host must be set.");
        StringBuilder args = new();
        args.Append(" " + Host);
        if (Port is not null)
            args.Append($" -p {Port}");
        if (User is not null)
            args.Append($" -l {User}");
        args.Append(" -o BatchMode=yes");
        args.Append(" -o StrictHostKeyChecking=yes");
        if (!string.IsNullOrEmpty(command))
            args.Append($" {command}");
        return args.ToString();
    }
}
