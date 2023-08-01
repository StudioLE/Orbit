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
    public const string ExampleServerName = "example-server";
    public const int ExampleServerNumber = 3;
    public const string ExampleNetworkName = "example-network";
    public const int ExampleNetworkNumber = 6;
    public const string ExampleInstanceName = "example-instance";
    public const int ExampleInstanceNumber = 9;

    private static Instance? _exampleInstance;

    public static Instance GetExampleInstance()
    {
        return _exampleInstance ?? throw new("Example instance must be created using TestHelpers.CreateTestHost()");
    }

    private static void CreateExampleServer(IServiceProvider services)
    {
        Server server = new()
        {
            Name = ExampleServerName,
            Number = ExampleServerNumber,
            Address = "localhost",
            Ssh = new()
            {
                Port = 22,
                User = "user",
                PrivateKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh/id_rsa")
            }
        };
        IEntityProvider<Server> servers = services.GetRequiredService<IEntityProvider<Server>>();
        servers.Put(server);
    }

    private static void CreateExampleNetwork(IServiceProvider services)
    {
        Network network = new()
        {
            Name = ExampleNetworkName,
            Number = ExampleNetworkNumber,
            Server = ExampleServerName,
            WireGuardPort = MockWireGuardFacade.Port,
            WireGuardPrivateKey = MockWireGuardFacade.PrivateKey,
            WireGuardPublicKey = MockWireGuardFacade.PublicKey,
            ExternalIPv4 = MockWireGuardFacade.ExternalIPv4,
            ExternalIPv6 = MockWireGuardFacade.ExternalIPv6,
            Dns = MockWireGuardFacade.Dns
        };
        IEntityProvider<Network> networks = services.GetRequiredService<IEntityProvider<Network>>();
        networks.Put(network);
    }

    private static void CreateExampleInstance(IServiceProvider services)
    {
        InstanceFactory instanceFactory = services.GetRequiredService<InstanceFactory>();
        Instance instance = instanceFactory.Create(new()
        {
            Name = ExampleInstanceName,
            Number = ExampleInstanceNumber,
            WireGuard = new[]
            {
                new WireGuard
                {
                    Network = ExampleServerName,
                    PrivateKey = MockWireGuardFacade.PrivateKey,
                    PublicKey = MockWireGuardFacade.PublicKey,
                    PreSharedKey = MockWireGuardFacade.PreSharedKey
                }
            },
            Domains = new[] { "example.com", "example.org" }
        });
        IEntityProvider<Instance> instances = services.GetRequiredService<IEntityProvider<Instance>>();
        instances.Put(instance);
        _exampleInstance = instance;
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
        CreateExampleServer(host.Services);
        CreateExampleNetwork(host.Services);
        CreateExampleInstance(host.Services);
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
