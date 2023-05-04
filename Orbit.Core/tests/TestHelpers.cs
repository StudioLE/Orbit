using Microsoft.Extensions.Hosting;
using Orbit.Core.Hosting;

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
                .AddTestEntityProvider())
            .Build();
    }
}
