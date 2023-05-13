using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging;

/// <summary>
/// Methods to help with <see cref="ILogger"/>.
/// </summary>
public static class LoggingHelpers
{

    /// <summary>
    /// Create a <see cref="ILogger{T}"/> that logs to the console.
    /// </summary>
    public static ILogger<T> CreateConsoleLogger<T>()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<T>();
    }
}
