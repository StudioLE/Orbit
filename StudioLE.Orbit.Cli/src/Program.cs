using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudioLE.Orbit.Clients;
using StudioLE.Orbit.CloudInit;
using StudioLE.Orbit.Configuration;
using StudioLE.Orbit.Hosting;
using StudioLE.Orbit.Instances;
using StudioLE.Orbit.Lxd;
using StudioLE.Orbit.Servers;
using StudioLE.Orbit.WireGuard;
using Tectonic.Extensions.CommandLine;
using Tectonic.Extensions.CommandLine.Utils;

namespace StudioLE.Orbit.Cli;

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
            .Register<InstanceUpdateActivity>("update", "instance")
            .Register<InstanceServerConfigActivity>("generate", "server-config", "instance")
            .Register<ClientServerConfigActivity>("generate", "server-config", "client")
            .Register<WireGuardClientActivity>("generate", "wireguard", "client")
            .Register<ServerConfigurationActivity>("execute", "server-config")
            .Register<LxdInitActivity>("init")
            .Build();

        await command.InvokeAsync(args);
    }
}
