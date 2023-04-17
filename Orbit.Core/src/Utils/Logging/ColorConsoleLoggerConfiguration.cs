using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging;

public sealed class ColorConsoleLoggerConfiguration
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Trace] = ConsoleColor.Gray,
        [LogLevel.Debug] = ConsoleColor.Gray,
        [LogLevel.Information] = ConsoleColor.Cyan,
        [LogLevel.Warning] = ConsoleColor.Yellow,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Critical] = ConsoleColor.Red,
        [LogLevel.None] = ConsoleColor.DarkMagenta
    };
}
