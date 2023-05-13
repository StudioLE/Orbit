using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbit.Core.Utils.Logging.ColorConsoleLogger;
using Orbit.Core.Utils.Logging.TestLogger;

namespace Orbit.Core.Hosting;

/// <summary>
/// Extensions to configure logging on an <see cref="IHostBuilder"/>.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Replace the loggers with <see cref="Microsoft.Extensions.Logging.Debug.DebugLogger"/>
    /// and <see cref="ColorConsoleLogger"/>.
    /// </summary>
    public static IHostBuilder UseCustomLoggingProviders(this IHostBuilder builder)
    {
        return builder
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddDebug();
                // logging.AddConsole();
                logging.AddColorConsoleLogger();
            });
    }

    /// <summary>
    /// Replace the loggers with <see cref="Microsoft.Extensions.Logging.Debug.DebugLogger"/>,
    /// <see cref="ColorConsoleLogger"/>, and <see cref="TestLogger"/>
    /// </summary>
    public static IHostBuilder UseTestLoggingProviders(this IHostBuilder builder)
    {
        return builder
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddDebug();
                // logging.AddConsole();
                logging.AddColorConsoleLogger();
                logging.AddTestLogger();
            });
    }
}
