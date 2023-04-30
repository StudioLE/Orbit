using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Activities;
using Orbit.Core.Providers;

namespace Orbit.Core.Hosting;

public static class ServiceExtensions
{
    public static IServiceCollection AddOrbitServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ProviderOptions>()
            .AddSingleton<EntityProvider>()
            .AddSingleton<CreateOptions>()
            .AddSingleton<Create>()
            .AddSingleton<Multipass>()
            .AddSingleton<Launch>();
    }

    public static IServiceCollection AddTestEntityProvider(this IServiceCollection services)
    {
        return services.AddSingleton<EntityProvider>(_ => EntityProvider.CreateTemp());
    }
}
