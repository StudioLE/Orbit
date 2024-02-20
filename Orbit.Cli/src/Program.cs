using System.CommandLine;
using Cascade.Workflows.CommandLine;
using Cascade.Workflows.CommandLine.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.CloudInit;
using Orbit.Configuration;
using Orbit.Creation.Clients;
using Orbit.Creation.Instances;
using Orbit.Creation.Networks;
using Orbit.Creation.Servers;
using Orbit.Hosting;
using Orbit.Multipass;
using Orbit.WireGuard;

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
            .Register<GenerateCloudInit>("generate", "cloud-init")
            .Register<GenerateServerConfigurationForInstance>("generate", "server-config", "instance")
            .Register<GenerateServerConfigurationForClient>("generate", "server-config", "client")
            .Register<GenerateWireGuardClient>("generate", "wireguard", "client")
            .Register<ExecuteServerConfiguration>("execute", "server-config")
            .Register<Launch>("launch")
            .Register<Mount>("mount")
            .Build();

        await command.InvokeAsync(args);
    }
}
