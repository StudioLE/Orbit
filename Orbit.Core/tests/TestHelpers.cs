using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Orbit.Core.Activities;
using Orbit.Core.Hosting;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Tests.Resources;

namespace Orbit.Core.Tests;

public static class TestHelpers
{
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
        IEntityProvider<Network> networks = host.Services.GetRequiredService<IEntityProvider<Network>>();
        networks.Put(new()
        {
            Name = "network-01",
            Number = 1
        });
        IEntityProvider<Server> servers = host.Services.GetRequiredService<IEntityProvider<Server>>();
        servers.Put(new()
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
        });
        return host;
    }

    private static IServiceCollection AddOrbitTestOptions(this IServiceCollection services)
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        return services
            .AddSingleton<IOptions<CreateOptions>>(_ => Options.Create<CreateOptions>(new()
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
