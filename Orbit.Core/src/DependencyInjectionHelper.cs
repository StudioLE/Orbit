using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Core.Activities;

namespace Orbit.Core;

public static class DependencyInjectionHelper
{
    public static IHostBuilder RegisterCreateServices(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddSingleton<CreateOptions>();
            services.AddSingleton<Create>();
        });
    }
}
