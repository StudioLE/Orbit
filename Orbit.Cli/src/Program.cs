using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Clients;
using Orbit.CloudInit;
using Orbit.Configuration;
using Orbit.Hosting;
using Orbit.Instances;
using Orbit.Lxd;
using Orbit.Servers;
using Orbit.WireGuard;
using Tectonic.Extensions.CommandLine;
using Tectonic.Extensions.CommandLine.Utils;

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
            .Register<ServerActivity>("create", "server")
            .Register<InstanceActivity>("create", "instance")
            .Register<ClientActivity>("create", "client")
            .Register<UserConfigActivity>("create", "user-config")
            .Register<LxdConfigActivity>("create", "lxd")
            .Register<InstanceServerConfigActivity>("generate", "server-config", "instance")
            .Register<ClientServerConfigActivity>("generate", "server-config", "client")
            .Register<GenerateWireGuardClient>("generate", "wireguard", "client")
            .Register<ServerConfigurationActivity>("execute", "server-config")
            .Register<LxdInitActivity>("init")
            .Build();

        await command.InvokeAsync(args);
    }
}
