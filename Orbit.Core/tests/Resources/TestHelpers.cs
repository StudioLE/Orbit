using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.CloudInit;
using Orbit.Creation.Clients;
using Orbit.Creation.Instances;
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
    private static Instance? _exampleInstance;
    private static Client? _exampleClient;

    public static Server GetExampleServer()
    {
        return _exampleServer ?? throw new("Example server must be created using TestHelpers.CreateTestHost()");
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
        Server server = factory
            .Create(new()
            {
                Name = new(MockConstants.ServerName),
                Number = MockConstants.ServerNumber,
                Ssh = new()
                {
                    Host = "localhost",
                    Port = 22,
                    User = "user"
                }
            })
            .WithMockMacAddress();
        IEntityProvider<Server> servers = services.GetRequiredService<IEntityProvider<Server>>();
        servers.Put(server);
        _exampleServer = server;
    }

    private static void CreateExampleInstance(IServiceProvider services)
    {
        InstanceFactory factory = services.GetRequiredService<InstanceFactory>();
        Instance instance = factory
            .Create(new()
            {
                Name = new(MockConstants.InstanceName),
                Number = MockConstants.InstanceNumber,
                Domains = ["example.com", "example.org"]
            })
            .WithMockMacAddress();
        IEntityProvider<Instance> instances = services.GetRequiredService<IEntityProvider<Instance>>();
        instances.Put(instance);
        _exampleInstance = instance;
    }

    private static void CreateExampleClient(IServiceProvider services)
    {
        ClientFactory factory = services.GetRequiredService<ClientFactory>();
        Client client = factory
            .Create(new()
            {
                Name = new(MockConstants.ClientName),
                Number = MockConstants.ClientNumber
            })
            .WithMockMacAddress();
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

    public static Server WithMockMacAddress(this Server server)
    {
        server.Interfaces = server
            .Interfaces
            .Select(WithMockMacAddress)
            .ToArray();
        return server;
    }

    public static T WithMockMacAddress<T>(this T entity) where T : struct, IHasWireGuardClient
    {
        entity.Interfaces = entity
            .Interfaces
            .Select(WithMockMacAddress)
            .ToArray();
        entity.WireGuard = entity
            .WireGuard
            .Select(WithMockMacAddress)
            .ToArray();
        return entity;
    }

    private static WireGuardClient WithMockMacAddress(WireGuardClient wg)
    {
        wg.Interface = WithMockMacAddress(wg.Interface);
        return wg;
    }

    private static Interface WithMockMacAddress(Interface iface)
    {
        iface.MacAddress = MockConstants.MacAddress;
        return iface;
    }
}
