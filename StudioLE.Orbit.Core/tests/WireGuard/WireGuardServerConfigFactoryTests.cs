using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using StudioLE.Orbit.Core.Tests.Resources;
using StudioLE.Orbit.Schema;
using StudioLE.Orbit.WireGuard;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Verify;

namespace StudioLE.Orbit.Core.Tests.WireGuard;

internal sealed class WireGuardServerConfigFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private WireGuardServerConfigFactory _wgConfigFactory = null!;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
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
