using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Orbit.Core.Shell.Sample;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = Host
            .CreateDefaultBuilder()
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddConsole();
            })
            .ConfigureServices((_, services) => services
                .AddTransient<ShellCommandSample>())
            .Build();
        ShellCommandSample shellCommandSample = host.Services.GetRequiredService<ShellCommandSample>();
        await shellCommandSample.Execute();
    }
}
