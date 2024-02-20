using Microsoft.Extensions.Logging;
using Orbit.Schema;
using Orbit.Utils.CommandLine;

namespace Orbit.Shell;

/// <summary>
/// Methods to help with Multipass.
/// </summary>
public static class MultipassHelpers
{
    /// <summary>
    /// Create a <see cref="Ssh"/> to execute Multipass commands on <see cref="Server"/>.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="server">The server to connect to.</param>
    /// <returns>The created <see cref="Ssh"/></returns>
    public static Ssh CreateSsh(ILogger logger, Server server)
    {
        return new()
        {
            Logger = logger,
            Host = server.Address,
            User = server.Ssh.User,
            Port = server.Ssh.Port,
            OnOutput = output => OutputToLogger(logger, output)
        };
    }

    /// <summary>
    /// Remove the rotating spinner and pass the standard output of a Multipass command to the logger.
    /// </summary>
    /// <param name="logger">The logger to pass to.</param>
    /// <param name="output">The standard output.</param>
    public static void OutputToLogger(ILogger logger, string output)
    {
        string filtered = RemoveMultipassSpinner(output);
        if (!string.IsNullOrEmpty(filtered))
            logger.LogInformation(filtered);
    }

    /// <summary>
    /// Filter the standard output of a Multipass command to remove the rotating spinner.
    /// </summary>
    /// <param name="output">The standard output of a Multipass command.</param>
    /// <returns>The output without spinner or unicode backspaces</returns>
    public static string RemoveMultipassSpinner(string output)
    {
        const char unicodeBackspace = '\b';
        return output
            .Replace(unicodeBackspace + "/", string.Empty)
            .Replace(unicodeBackspace + "-", string.Empty)
            .Replace(unicodeBackspace + @"\", string.Empty)
            .Replace(unicodeBackspace + "-", string.Empty)
            .Replace(unicodeBackspace + "|", string.Empty)
            .Replace(unicodeBackspace.ToString(), string.Empty);
    }
}
