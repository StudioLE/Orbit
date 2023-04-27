using System.CommandLine;
using System.CommandLine.IO;
using Microsoft.Extensions.Logging;

namespace StudioLE.CommandLine.Logging;

public class RedirectConsoleToLogger : IConsole
{
    /// <inheritdoc />
    public IStandardStreamWriter Out { get; }

    /// <inheritdoc />
    public bool IsOutputRedirected { get; } = true;

    /// <inheritdoc />
    public IStandardStreamWriter Error { get; }

    /// <inheritdoc />
    public bool IsErrorRedirected { get; } = true;

    /// <inheritdoc />
    public bool IsInputRedirected { get; } = false;

    public RedirectConsoleToLogger(ILogger logger)
    {
        Out = new LogWriter(logger, LogLevel.Information);
        Error = new LogWriter(logger, LogLevel.Error);
    }
    public void Flush()
    {
        ((LogWriter)Out).Flush();
        ((LogWriter)Error).Flush();
    }
}
