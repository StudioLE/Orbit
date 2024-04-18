using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orbit.Caddy;
using Orbit.Clients;
using Orbit.CloudInit;
using Orbit.Configuration;
using Orbit.Instances;
using Orbit.Lxd;
using Orbit.Provision;
using Orbit.Serialization;
using Orbit.Servers;
using Orbit.Utils.CommandLine;
using Orbit.WireGuard;
using StudioLE.Storage.Files;

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

            .AddSingleton<IOptions<PhysicalFileSystemOptions>>(services =>
            {
                IOptions<ProviderOptions> providerOptions = services.GetRequiredService<IOptions<ProviderOptions>>();
                PhysicalFileSystemOptions options = new()
                {
                    RootDirectory = providerOptions.Value.Directory
                };
                return Options.Create(options);
            })

            .AddSingleton<IOptions<PhysicalFileWriterOptions>>(_ => Options.Create(new PhysicalFileWriterOptions
            {
                AllowOverwrite = true,
                AllowSubDirectories = true
            }))

            .AddTransient(YamlHelpers.CreateSerializer)
            .AddTransient(YamlHelpers.CreateDeserializer)

            .AddTransient<IFileReader, PhysicalFileReader>()
            .AddTransient<IFileWriter, PhysicalFileWriter>()
            .AddTransient<IDirectoryReader, PhysicalDirectoryReader>()
            .AddTransient<InstanceProvider>()
            .AddTransient<ServerProvider>()
            .AddTransient<ClientProvider>()

            .AddTransient<ServerActivity>()
            .AddTransient<ServerFactory>()
            .AddTransient<WireGuardServerFactory>()
            .AddTransient<BridgeInterfaceFactory>()

            .AddTransient<InstanceActivity>()
            .AddTransient<InstanceFactory>()
            .AddTransient<HardwareFactory>()
            .AddTransient<ExternalInterfaceFactory>()
            .AddTransient<InternalInterfaceFactory>()
            .AddTransient<WireGuardInterfaceFactory>()
            .AddTransient<OSFactory>()
            .AddTransient<WireGuardClientFactory>()
            .AddTransient<IWireGuardFacade, WireGuardFacade>()

            .AddTransient<InstanceUpdateActivity>()

            .AddTransient<ClientActivity>()
            .AddTransient<ClientFactory>()

            .AddTransient<UserConfigActivity>()
            .AddTransient<UserConfigProvider>()
            .AddTransient<UserConfigFactory>()
            .AddTransient<InstallFactory>()
            .AddTransient<RunFactory>()
            .AddTransient<NetplanFactory>()
            .AddTransient<WireGuardClientConfigFactory>()

            .AddTransient<LxdConfigActivity>()
            .AddTransient<LxdConfigFactory>()
            .AddTransient<LxdConfigProvider>()

            .AddTransient<WireGuardClientActivity>()
            .AddTransient<WireGuardConfigProvider>()
            .AddTransient<IQREncodeFacade, QREncodeFacade>()

            .AddTransient<InstanceServerConfigActivity>()
            .AddTransient<ServerConfigurationProvider>()
            .AddTransient<ClientServerConfigActivity>()
            .AddTransient<CaddyfileFactory>()
            .AddTransient<WriteCaddyfileCommandFactory>()
            .AddTransient<WireGuardSetCommandFactory>()
            .AddTransient<WireGuardServerConfigFactory>()

            .AddTransient<ServerConfigurationActivity>()

            .AddTransient<LxdInitActivity>()

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
