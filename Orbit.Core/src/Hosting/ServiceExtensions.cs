using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orbit.Creation;
using Orbit.Generation;
using Orbit.Initialization;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Shell;
using StudioLE.Serialization;
using StudioLE.Serialization.Yaml;
using Mount = Orbit.Initialization.Mount;

namespace Orbit.Hosting;

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
        // @formatter:off

            .AddTransient<ISerializer, YamlSerializer>()
            .AddTransient<IDeserializer, YamlDeserializer>()

            .AddTransient<IEntityFileProvider, EntityFileProvider>()
            .AddTransient<IEntityProvider<Instance>, EntityProvider<Instance>>()
            .AddTransient<IEntityProvider<Network>, EntityProvider<Network>>()
            .AddTransient<IEntityProvider<Server>, EntityProvider<Server>>()
            .AddTransient<IEntityProvider<Client>, EntityProvider<Client>>()

            .AddScoped<CommandContext>()

            .AddTransient<CreateServer>()
            .AddTransient<ServerFactory>()

            .AddTransient<CreateNetwork>()
            .AddTransient<NetworkFactory>()
            .AddTransient<WireGuardServerFactory>()

            .AddTransient<CreateInstance>()
            .AddTransient<InstanceFactory>()
            .AddTransient<HardwareFactory>()
            .AddTransient<NetworkFactory>()
            .AddTransient<OSFactory>()
            .AddTransient<WireGuardClientFactory>()
            .AddTransient<IWireGuardFacade, WireGuardFacade>()
            .AddTransient<IIPAddressStrategy, IPAddressStrategy>()

            .AddTransient<CreateClient>()
            .AddTransient<ClientFactory>()

            .AddTransient<GenerateInstanceConfiguration>()
            .AddTransient<CloudInitFactory>()
            .AddTransient<InstallFactory>()
            .AddTransient<RunFactory>()
            .AddTransient<NetplanFactory>()
            .AddTransient<WireGuardClientConfigFactory>()

            .AddTransient<GenerateClientConfiguration>()
            .AddTransient<QREncodeFacade>()

            .AddTransient<GenerateServerConfiguration>()
            .AddTransient<GenerateServerConfigurationForInstance>()
            .AddTransient<GenerateServerConfigurationForClient>()
            .AddTransient<GenerateServerConfiguration>()
            .AddTransient<CaddyfileFactory>()
            .AddTransient<WriteCaddyfileCommandFactory>()
            .AddTransient<WireGuardSetCommandFactory>()
            .AddTransient<MountCommandFactory>()
            .AddTransient<CloneRepoCommandFactory>()
            .AddTransient<WireGuardServerConfigFactory>()

            .AddTransient<Initialize>()

            .AddTransient<Launch>()

            .AddTransient<Mount>();

        // @formatter:on
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
