using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orbit.Cli.Shell;
using Orbit.Cli.Utils.Converters;
using Orbit.Core.Schema;

namespace Orbit.Cli;

public static class DependencyInjectionHelper
{
    public static IHostBuilder RegisterRemoteServices(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddLogging(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug));
            services.AddSingleton<HostShellOptions>();
            services.AddSingleton<HostShell>();
        });
    }

    public static ConverterResolver DefaultConverterResolver()
    {
        return new ConverterResolverBuilder()
            .Register<int, StringToInteger>()
            .Register<double, StringToDouble>()
            .Register<Platform, StringToEnum<Platform>>()
            .Build();
    }
}
