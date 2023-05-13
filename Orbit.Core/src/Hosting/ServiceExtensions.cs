using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Activities;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Shell;

namespace Orbit.Core.Hosting;

/// <summary>
/// Extensions to configure services on an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Add the DI services required by Orbit
    /// </summary>
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

    /// <summary>
    /// Add <see cref="EntityProvider.CreateTemp()"/> as the <see cref="EntityProvider"/>.
    /// </summary>
    public static IServiceCollection AddTestEntityProvider(this IServiceCollection services)
    {
        return services.AddSingleton<EntityProvider>(_ => EntityProvider.CreateTemp());
    }
}
