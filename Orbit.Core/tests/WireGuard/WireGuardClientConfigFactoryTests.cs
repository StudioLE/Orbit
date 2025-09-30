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

internal sealed class WireGuardClientConfigFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private WireGuardClientFactory _wireGuardClientFactory = null!;
    private WireGuardClientConfigFactory _wgConfigFactory = null!;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
        _wireGuardClientFactory = host.Services.GetRequiredService<WireGuardClientFactory>();
        _wgConfigFactory = host.Services.GetRequiredService<WireGuardClientConfigFactory>();
    }

    [Test]
    [Category("Factory")]
    public async Task WireGuardClientConfigFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        WireGuardClient[] interfaces = await _wireGuardClientFactory.Create(instance);
        WireGuardClient wg = interfaces.First();

        // Act
        string output = await _wgConfigFactory.Create(wg);

        // Assert
        await _context.Verify(output);
    }
}
