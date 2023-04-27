using Microsoft.Extensions.Logging;

namespace StudioLE.CommandLine.Utils.Logging;

public static class LoggingHelpers
{
    public static ILogger<T> CreateConsoleLogger<T>()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<T>();
    }
}
