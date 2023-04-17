using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbit.Core.Activities;
using Orbit.Core.SSH;
using Orbit.Core.Utils.Logging;

namespace Orbit.Core;

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

    public static IHostBuilder RegisterCreateServices(this IHostBuilder builder)
    {
        return builder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ProviderOptions>();
                services.AddSingleton<InstanceProvider>();
                services.AddSingleton<CreateOptions>();
                services.AddSingleton<Create>();
            });
    }

    public static IHostBuilder RegisterLaunchServices(this IHostBuilder builder)
    {
        return builder
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<ProviderOptions>();
                services.AddSingleton<InstanceProvider>();
                services.AddSingleton<ConnectionOptions>();
                services.AddSingleton<Multipass>();
                services.AddSingleton<Launch>();
            });
    }
}
