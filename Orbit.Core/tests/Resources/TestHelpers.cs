using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.Clients;
using Orbit.CloudInit;
using Orbit.Hosting;
using Orbit.Instances;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Servers;
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

    private static async Task CreateExampleServer(IServiceProvider services)
    {
        ServerFactory factory = services.GetRequiredService<ServerFactory>();
        Server server = await factory
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
            });
        ServerProvider servers = services.GetRequiredService<ServerProvider>();
        await servers.Put(server);
        _exampleServer = server;
    }

    private static async Task CreateExampleInstance(IServiceProvider services)
    {
        InstanceFactory factory = services.GetRequiredService<InstanceFactory>();
        Instance instance = await factory
            .Create(new()
            {
                Name = new(MockConstants.InstanceName),
                Number = MockConstants.InstanceNumber,
                Domains = ["example.com", "example.org"]
            });
        InstanceProvider instances = services.GetRequiredService<InstanceProvider>();
        await instances.Put(instance);
        _exampleInstance = instance;
    }

    private static async Task CreateExampleClient(IServiceProvider services)
    {
        ClientFactory factory = services.GetRequiredService<ClientFactory>();
        Client client = await factory
            .Create(new()
            {
                Name = new(MockConstants.ClientName),
                Number = MockConstants.ClientNumber
            });
        ClientProvider clients = services.GetRequiredService<ClientProvider>();
        await clients.Put(client);
        _exampleClient = client;
    }

    public static async Task<IHost> CreateTestHost(Action<IServiceCollection>? configureServices = null)
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
        await CreateExampleServer(host.Services);
        await CreateExampleInstance(host.Services);
        await CreateExampleClient(host.Services);
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
