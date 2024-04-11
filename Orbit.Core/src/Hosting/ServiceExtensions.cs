using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orbit.Caddy;
using Orbit.CloudInit;
using Orbit.Configuration;
using Orbit.Creation.Clients;
using Orbit.Creation.Instances;
using Orbit.Creation.Servers;
using Orbit.Lxd;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Serialization;
using Orbit.Utils.CommandLine;
using Orbit.WireGuard;
using Tectonic;

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

            .AddTransient(YamlHelpers.CreateSerializer)
            .AddTransient(YamlHelpers.CreateDeserializer)

            .AddTransient<IEntityFileProvider, EntityFileProvider>()
            .AddTransient<IEntityProvider<Instance>, EntityProvider<Instance>>()
            .AddTransient<IEntityProvider<Server>, EntityProvider<Server>>()
            .AddTransient<IEntityProvider<Client>, EntityProvider<Client>>()

            .AddScoped<CommandContext>()

            .AddTransient<CreateServer>()
            .AddTransient<ServerFactory>()
            .AddTransient<WireGuardServerFactory>()
            .AddTransient<BridgeInterfaceFactory>()

            .AddTransient<CreateInstance>()
            .AddTransient<InstanceFactory>()
            .AddTransient<HardwareFactory>()
            .AddTransient<ExternalInterfaceFactory>()
            .AddTransient<InternalInterfaceFactory>()

            .AddTransient<OSFactory>()
            .AddTransient<WireGuardClientFactory>()
            .AddTransient<IWireGuardFacade, WireGuardFacade>()

            .AddTransient<CreateClient>()
            .AddTransient<ClientFactory>()

            .AddTransient<GenerateUserConfig>()
            .AddTransient<UserConfigFactory>()
            .AddTransient<InstallFactory>()
            .AddTransient<RunFactory>()
            .AddTransient<NetplanFactory>()
            .AddTransient<WireGuardClientConfigFactory>()

            .AddTransient<GenerateLxdConfig>()
            .AddTransient<LxdConfigFactory>()

            .AddTransient<GenerateWireGuardClient>()
            .AddTransient<IQREncodeFacade, QREncodeFacade>()

            .AddTransient<GenerateServerConfigurationForInstance>()
            .AddTransient<GenerateServerConfigurationForClient>()
            .AddTransient<CaddyfileFactory>()
            .AddTransient<WriteCaddyfileCommandFactory>()
            .AddTransient<WireGuardSetCommandFactory>()
            .AddTransient<WireGuardServerConfigFactory>()

            .AddTransient<ExecuteServerConfiguration>()

            .AddTransient<Init>()

            .AddTransient<Cli>()
            .AddTransient<Ssh>()
            ;

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
