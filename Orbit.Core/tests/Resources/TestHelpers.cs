using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.CloudInit;
using Orbit.Creation.Clients;
using Orbit.Creation.Instances;
using Orbit.Creation.Networks;
using Orbit.Creation.Servers;
using Orbit.Hosting;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.WireGuard;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Extensions.Logging.Console;

namespace Orbit.Core.Tests.Resources;

public static class TestHelpers
{
    private static Server? _exampleServer;
    private static Network? _exampleNetwork;
    private static Instance? _exampleInstance;
    private static Client? _exampleClient;

    public static Server GetExampleServer()
    {
        return _exampleServer ?? throw new("Example server must be created using TestHelpers.CreateTestHost()");
    }

    public static Network GetExampleNetwork()
    {
        return _exampleNetwork ?? throw new("Example network must be created using TestHelpers.CreateTestHost()");
    }

    public static Instance GetExampleInstance()
    {
        return _exampleInstance ?? throw new("Example instance must be created using TestHelpers.CreateTestHost()");
    }

    public static Client GetExampleClient()
    {
        return _exampleClient ?? throw new("Example client must be created using TestHelpers.CreateTestHost()");
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
        _exampleServer = server;
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
            WireGuard = new()
            {
                PrivateKey = MockConstants.PrivateKey,
                PublicKey = MockConstants.PublicKey
            }
        });
        IEntityProvider<Network> networks = services.GetRequiredService<IEntityProvider<Network>>();
        networks.Put(network);
        _exampleNetwork = network;
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

    private static void CreateExampleClient(IServiceProvider services)
    {
        ClientFactory factory = services.GetRequiredService<ClientFactory>();
        Client client = factory.Create(new()
        {
            Name = MockConstants.ClientName,
            Number = MockConstants.ClientNumber,
            WireGuard = new[]
            {
                new WireGuardClient
                {
                    Network = MockConstants.NetworkName,
                    PrivateKey = MockConstants.PrivateKey,
                    PublicKey = MockConstants.PublicKey,
                    PreSharedKey = MockConstants.PreSharedKey
                }
            }
        });
        IEntityProvider<Client> clients = services.GetRequiredService<IEntityProvider<Client>>();
        clients.Put(client);
        _exampleClient = client;
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
                .AddMockWireGuardFacade()
                .AddMockQREncodeFacade())
            .ConfigureServices(configureServices)
            .Build();
        CreateExampleServer(host.Services);
        CreateExampleNetwork(host.Services);
        CreateExampleInstance(host.Services);
        CreateExampleClient(host.Services);
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

    // ReSharper disable once InconsistentNaming
    private static IServiceCollection AddMockQREncodeFacade(this IServiceCollection services)
    {
        return services.AddSingleton<IQREncodeFacade, MockQREncodeFacade>();
    }

    /// <summary>
    /// Replace the loggers with <see cref="Microsoft.Extensions.Logging.Debug.DebugLogger"/>,
    /// <see cref="BasicConsoleFormatter"/>  and <see cref="CacheLogger"/>
    /// </summary>
    public static IHostBuilder UseTestLoggingProviders(this IHostBuilder builder)
    {
        return builder
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddBasicConsole();
                // logging.AddConsole();
                logging.AddCache();
            });
    }
}
