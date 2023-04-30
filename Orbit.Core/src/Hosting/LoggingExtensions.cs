using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbit.Core.Utils.Logging.ColorConsoleLogger;
using Orbit.Core.Utils.Logging.TestLogger;

namespace Orbit.Core.Hosting;

public static class LoggingExtensions
{
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
