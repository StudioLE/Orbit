using System.CommandLine;
using Cascade.Workflows.CommandLine;
using Cascade.Workflows.CommandLine.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.CloudInit;
using Orbit.Configuration;
using Orbit.Creation.Clients;
using Orbit.Creation.Instances;
using Orbit.Creation.Servers;
using Orbit.Hosting;
using Orbit.Lxd;
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
            .Register<CreateInstance>("create", "instance")
            .Register<CreateClient>("create", "client")
            .Register<GenerateUserConfig>("generate", "user-config")
            .Register<GenerateLxdConfig>("generate", "lxd")
            .Register<GenerateServerConfigurationForInstance>("generate", "server-config", "instance")
            .Register<GenerateServerConfigurationForClient>("generate", "server-config", "client")
            .Register<GenerateWireGuardClient>("generate", "wireguard", "client")
            .Register<ExecuteServerConfiguration>("execute", "server-config")
            .Register<Init>("init")
            .Build();

        await command.InvokeAsync(args);
    }
}
