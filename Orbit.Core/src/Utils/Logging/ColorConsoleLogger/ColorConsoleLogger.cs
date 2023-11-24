using Microsoft.Extensions.Logging;
using StudioLE.Extensions.System.Exceptions;

namespace Orbit.Core.Utils.Logging.ColorConsoleLogger;

// TODO: Rename to CustomConsoleLogger
/// <summary>
/// A logger that writes to the console in colors appropriate to the <see cref="LogLevel"/>.
/// </summary>
public sealed class ColorConsoleLogger : ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        ConsoleColor originalColor = Console.ForegroundColor;

        Console.ForegroundColor = GetColorForLevel(logLevel);
        Console.Write($"{formatter(state, exception)}");

        Console.ForegroundColor = originalColor;
        Console.WriteLine();
    }

    private static ConsoleColor GetColorForLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Information => ConsoleColor.Cyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.Red,
            LogLevel.None => ConsoleColor.DarkMagenta,
            _ => throw new EnumSwitchException<LogLevel>("Failed to get ConsoleColor", logLevel)
        };
    }
}
