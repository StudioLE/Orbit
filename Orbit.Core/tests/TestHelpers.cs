using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orbit.Core.Creation;
using Orbit.Core.Generation;
using Orbit.Core.Hosting;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Tests.Resources;

namespace Orbit.Core.Tests;

public static class TestHelpers
{
    public static Instance ExampleInstance()
    {
        return new()
        {
            Server = "server-01",
            Networks = new [] { "network-01" },
            WireGuard = new[]
            {
                new WireGuard
                {
                    Network = "network-01",
                    PrivateKey = MockWireGuardFacade.PrivateKey,
                    PublicKey = MockWireGuardFacade.PublicKey,
                    PreSharedKey = MockWireGuardFacade.PreSharedKey
                }
            }
        };
    }

    private static Server ExampleServer()
    {
        return new()
        {
            Name = "server-01",
            Number = 1,
            Address = "localhost",
            Ssh = new()
            {
                Port = 22,
                User = "user",
                PrivateKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh/id_rsa")
            }
        };
    }

    private static Network ExampleNetwork()
    {
        return new()
        {
            Name = "network-01",
            Number = 1,
            Server = "server-01",
            WireGuardPort = MockWireGuardFacade.Port,
            WireGuardPrivateKey = MockWireGuardFacade.PrivateKey,
            WireGuardPublicKey = MockWireGuardFacade.PublicKey,
            ExternalIPv4 = MockWireGuardFacade.ExternalIPv4,
            ExternalIPv6 = MockWireGuardFacade.ExternalIPv6,
            Dns = "10.1.1.0"
        };
    }

    public static IHost CreateTestHost(Action<IServiceCollection>? configureServices = null)
    {
        configureServices ??= _ => { };
        IHost host = Host
            .CreateDefaultBuilder()
            .UseTestLoggingProviders()
            .ConfigureServices(services => services
                // .AddOrbitOptions(context.Configuration)
                .AddOrbitTestOptions()
                .AddOrbitServices()
                .AddMockWireGuardFacade())
            .ConfigureServices(configureServices)
            .Build();
        IEntityProvider<Server> servers = host.Services.GetRequiredService<IEntityProvider<Server>>();
        servers.Put(ExampleServer());
        IEntityProvider<Network> networks = host.Services.GetRequiredService<IEntityProvider<Network>>();
        networks.Put(ExampleNetwork());
        IEntityProvider<Instance> instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
        InstanceFactory instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        Instance instance = instanceFactory.Create(new());
        instance.Domains = new[] { "example.com", "example.org" };
        instances.Put(instance);
        return host;
    }

    private static IServiceCollection AddOrbitTestOptions(this IServiceCollection services)
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        return services
            .AddSingleton<IOptions<CloudInitOptions>>(_ => Options.Create<CloudInitOptions>(new()
            {
                SudoUser = "admin",
                User = "user"
            }))
            .AddSingleton<IOptions<ProviderOptions>>(_ => Options.Create<ProviderOptions>(new()
            {
                Directory = directory
            }));
    }

    private static IServiceCollection AddMockWireGuardFacade(this IServiceCollection services)
    {
        return services.AddSingleton<IWireGuardFacade, MockWireGuardFacade>();
    }
}
