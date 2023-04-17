using Microsoft.Extensions.Logging;
using StudioLE.Core.Exceptions;

namespace Orbit.Core.Utils.Logging.ColorConsoleLogger;

public sealed class ColorConsoleLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

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
