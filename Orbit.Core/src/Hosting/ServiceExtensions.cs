using Microsoft.Extensions.Configuration;
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
            .AddSingleton<EntityProvider>()
            .AddSingleton<IWireGuardFacade, WireGuardFacade>()
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

    /// <summary>
    /// Add the options required by Orbit.
    /// </summary>
    /// <param name="services">A service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns><paramref name="services"/> for fluent chaining.</returns>
    public static IServiceCollection AddOrbitOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<CreateOptions>()
            .Bind(configuration.GetSection(CreateOptions.SectionKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services
            .AddOptions<ProviderOptions>()
            .Bind(configuration.GetSection(ProviderOptions.SectionKey))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
