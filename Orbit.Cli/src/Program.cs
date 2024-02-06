using System.CommandLine;
using Cascade.Workflows.CommandLine;
using Cascade.Workflows.CommandLine.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Creation;
using Orbit.Generation;
using Orbit.Hosting;
using Orbit.Initialization;

namespace Orbit.Cli;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        IHost host = Host
            .CreateDefaultBuilder()
            .RegisterCustomLoggingProviders()
            .ConfigureServices((context, services) => services
                .AddOrbitOptions(context.Configuration)
                .AddOrbitServices()
                .AddCommandBuilderServices())
            .Build();
        CommandBuilder builder = host
            .Services
            .GetRequiredService<CommandBuilder>();
        RootCommand command = builder
            .Register<CreateServer>("create", "server")
            .Register<CreateNetwork>("create", "network")
            .Register<CreateInstance>("create", "instance")
            .Register<CreateClient>("create", "client")
            .Register<GenerateInstanceConfiguration>("generate", "instance")
            .Register<GenerateServerConfiguration>("generate", "server")
            .Register<GenerateClientConfiguration>("generate", "client")
            .Register<Initialize>("initialize")
            .Register<Launch>("launch")
            .Register<Mount>("mount")
            .Build();

        await command.InvokeAsync(args);
    }
}
