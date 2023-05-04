using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Core.Hosting;
using Orbit.Core.Shell;
using Orbit.Core.Tests.Resources;

namespace Orbit.Core.Tests;

public static class TestHelpers
{
    public static IHost CreateTestHost()
    {
        return Host
            .CreateDefaultBuilder()
            .UseTestLoggingProviders()
            .ConfigureServices(services => services
                .AddOrbitServices()
                .AddMockWireGuardFacade()
                .AddTestEntityProvider())
            .Build();
    }

    public static IServiceCollection AddMockWireGuardFacade(this IServiceCollection services)
    {
        return services.AddSingleton<IWireGuardFacade, MockWireGuardFacade>();
    }
}
