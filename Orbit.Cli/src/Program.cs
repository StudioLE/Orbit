using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Core;
using Orbit.Core.Activities;
using StudioLE.CommandLine;

namespace Orbit.Cli;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = Host
            .CreateDefaultBuilder()
            .RegisterCustomLoggingProviders()
            .RegisterServices()
            .ConfigureServices(services => services
                .AddCommandBuilderServices())
            .Build();
        CommandBuilder builder = host
            .Services
            .GetRequiredService<CommandBuilder>();
        RootCommand command = builder
            .Register<Create>()
            // .Register<Launch>()
            .Build();

        await command.InvokeAsync(args);
    }
}
