using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Activities;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Shell;

namespace Orbit.Core.Hosting;

public static class ServiceExtensions
{
    public static IServiceCollection AddOrbitServices(this IServiceCollection services)
    {
        return services
            .AddTransient<InstanceFactory>()
            .AddTransient<HardwareFactory>()
            .AddTransient<NetworkFactory>()
            .AddTransient<OSFactory>()
            .AddTransient<WireGuardFactory>()
            .AddSingleton<ProviderOptions>()
            .AddSingleton<EntityProvider>()
            .AddSingleton<IWireGuardFacade, WireGuardFacade>()
            .AddSingleton<CreateOptions>()
            .AddSingleton<Create>()
            .AddSingleton<Launch>();
    }

    public static IServiceCollection AddTestEntityProvider(this IServiceCollection services)
    {
        return services.AddSingleton<EntityProvider>(_ => EntityProvider.CreateTemp());
    }
}
