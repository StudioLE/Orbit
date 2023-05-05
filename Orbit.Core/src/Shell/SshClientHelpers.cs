using Microsoft.Extensions.Logging;
using Orbit.Core.Utils;
using Renci.SshNet;

// ReSharper disable CommentTypo

namespace Orbit.Core.Shell;

public static class SshClientHelpers
{
    public static string? ExecuteToLogger(this SshClient ssh, ILogger logger, string commandText)
    {
        SshCommand command = ssh.CreateCommand(commandText);
        IAsyncResult result = command.BeginExecute();
        using StreamReader reader = new(command.OutputStream);
        while (!result.IsCompleted || !reader.EndOfStream)
        {
            // string output = reader.ReadToEnd();
            string? line = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;
            line = line
                .Replace("\u0008/", "")
                .Replace("\u0008-", "")
                .Replace("\u0008\\", "")
                .Replace("\u0008|", "")
                .Replace("\u0008", "");
            if (string.IsNullOrWhiteSpace(line))
                continue;
            logger.LogInformation(line);
        }
        command.EndExecute(result);

        if (command.ExitStatus == 0)
            return command.Result;
        logger.LogError("Failed to get multipass info.");
        if (!command.Error.IsNullOrEmpty())
            logger.LogError(command.Error);
        if (!command.Error.IsNullOrEmpty())
            logger.LogError(command.Error);
        return null;
    }

    public static string? Execute(this SshClient ssh, ILogger logger, string commandText)
    {
        SshCommand command = ssh.RunCommand(commandText);
        if (command.ExitStatus == 0)
            return command.Result;
        logger.LogError("Failed to get multipass info.");
        if (!command.Error.IsNullOrEmpty())
            logger.LogError(command.Error);
        if (!command.Error.IsNullOrEmpty())
            logger.LogError(command.Error);
        return null;
    }
}
