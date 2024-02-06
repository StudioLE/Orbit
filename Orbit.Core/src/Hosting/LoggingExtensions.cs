using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudioLE.Extensions.Logging.Console;

namespace Orbit.Hosting;

/// <summary>
/// Extensions to configure logging on an <see cref="IHostBuilder"/>.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// and <see cref="BasicConsoleFormatter"/>.
    /// </summary>
    public static IHostBuilder UseCustomLoggingProviders(this IHostBuilder builder)
    {
        return builder
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                // logging.AddConsole();
                logging.AddBasicConsole();
            });
    }
}
