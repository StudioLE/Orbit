using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orbit.Core.Creation;
using Orbit.Core.Generation;
using Orbit.Core.Hosting;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Shell;

namespace Orbit.Core.Tests.Resources;

public static class TestHelpers
{
    private static Instance? _exampleInstance;

    public static Instance GetExampleInstance()
    {
        return _exampleInstance ?? throw new("Example instance must be created using TestHelpers.CreateTestHost()");
    }

    private static void CreateExampleServer(IServiceProvider services)
    {
        ServerFactory factory = services.GetRequiredService<ServerFactory>();
        Server server = factory.Create(new()
        {
            Name = MockConstants.ServerName,
            Number = MockConstants.ServerNumber,
            Address = "localhost",
            Ssh = new()
            {
                Port = 22,
                User = "user",
                PrivateKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh/id_rsa")
            }
        });
        IEntityProvider<Server> servers = services.GetRequiredService<IEntityProvider<Server>>();
        servers.Put(server);
    }

    private static void CreateExampleNetwork(IServiceProvider services)
    {
        NetworkFactory factory = services.GetRequiredService<NetworkFactory>();
        Network network = factory.Create(new()
        {
            Name = MockConstants.NetworkName,
            Number = MockConstants.NetworkNumber,
            Server = MockConstants.ServerName,
            ExternalIPv4 = MockConstants.ExternalIPv4,
            ExternalIPv6 = MockConstants.ExternalIPv6,
            Dns = MockConstants.Dns,
            WireGuard = new()
            {
                PrivateKey = MockConstants.PrivateKey,
                PublicKey = MockConstants.PublicKey
            }
        });

        IEntityProvider<Network> networks = services.GetRequiredService<IEntityProvider<Network>>();
        networks.Put(network);
    }

    private static void CreateExampleInstance(IServiceProvider services)
    {
        InstanceFactory factory = services.GetRequiredService<InstanceFactory>();
        Instance instance = factory.Create(new()
        {
            Name = MockConstants.InstanceName,
            Number = MockConstants.InstanceNumber,
            MacAddress = MockConstants.MacAddress,
            WireGuard = new[]
            {
                new WireGuardClient
                {
                    Network = MockConstants.NetworkName,
                    PrivateKey = MockConstants.PrivateKey,
                    PublicKey = MockConstants.PublicKey,
                    PreSharedKey = MockConstants.PreSharedKey
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
