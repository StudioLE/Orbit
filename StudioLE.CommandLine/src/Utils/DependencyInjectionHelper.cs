using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudioLE.CommandLine.Utils.Logging.ColorConsoleLogger;
using StudioLE.CommandLine.Utils.Logging.TestLogger;

namespace StudioLE.CommandLine.Utils;

public static class DependencyInjectionHelper
{

    public static IHostBuilder RegisterCustomLoggingProviders(this IHostBuilder builder)
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

    public static IHostBuilder RegisterTestLoggingProviders(this IHostBuilder builder)
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
