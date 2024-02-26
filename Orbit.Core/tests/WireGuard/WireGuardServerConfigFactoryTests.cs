using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using Orbit.WireGuard;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Verify;

namespace Orbit.Core.Tests.WireGuard;

internal sealed class WireGuardServerConfigFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly WireGuardServerConfigFactory _wgConfigFactory;

    public WireGuardServerConfigFactoryTests()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = TestHelpers.CreateTestHost();
        _wgConfigFactory = host.Services.GetRequiredService<WireGuardServerConfigFactory>();
    }

    [Test]
    [Category("Factory")]
    public async Task WireGuardServerConfigFactory_Create()
    {
        // Arrange
        Server server = TestHelpers.GetExampleServer();

        // Act
        string output = _wgConfigFactory.Create(server);

        // Assert
        await _context.Verify(output);
    }
}
