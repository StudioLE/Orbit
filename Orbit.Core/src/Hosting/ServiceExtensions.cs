using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Activities;
using Orbit.Core.Creation;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Utils.Serialization.Yaml;
using StudioLE.Core.Serialization;

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

            .AddTransient<ISerializer, YamlSerializer>()
            .AddTransient<IDeserializer, YamlDeserializer>()

            .AddTransient<IEntityFileProvider, EntityFileProvider>()
            .AddTransient<IEntityProvider<Instance>, EntityProvider<Instance>>()
            .AddTransient<IEntityProvider<Network>, EntityProvider<Network>>()
            .AddTransient<IEntityProvider<Server>, EntityProvider<Server>>()

            .AddSingleton<CommandContext>()

            .AddSingleton<CreateInstance>()
            .AddTransient<InstanceFactory>()
            .AddTransient<HardwareFactory>()
            .AddTransient<NetworkFactory>()
            .AddTransient<OSFactory>()
            .AddTransient<WireGuardFactory>()
            .AddSingleton<IWireGuardFacade, WireGuardFacade>()

            .AddSingleton<GenerateInstanceConfiguration>()
            .AddTransient<CloudInitFactory>()

            .AddSingleton<GenerateServerConfiguration>()
            .AddTransient<CaddyfileFactory>()
            .AddTransient<WriteCaddyfileCommandFactory>()
            .AddTransient<WireGuardConfigFactory>()
            .AddTransient<WireGuardSetCommandFactory>()
            .AddTransient<MountCommandFactory>()

            .AddSingleton<Launch>()
            .AddTransient<LaunchCommandFactory>()

            .AddSingleton<Pull>();
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
            .AddOptions<CloudInitOptions>()
            .Bind(configuration.GetSection(CloudInitOptions.SectionKey))
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
