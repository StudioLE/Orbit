using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Creation.Clients;
using Orbit.Schema;
using Orbit.WireGuard;
using StudioLE.Verify;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class WireGuardClientConfigFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly WireGuardClientFactory _wireGuardClientFactory;
    private readonly WireGuardClientConfigFactory _wgConfigFactory;

    public WireGuardClientConfigFactoryTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _wireGuardClientFactory = host.Services.GetRequiredService<WireGuardClientFactory>();
        _wgConfigFactory = host.Services.GetRequiredService<WireGuardClientConfigFactory>();
    }

    [Test]
    [Category("Factory")]
    public async Task WireGuardClientConfigFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        WireGuardClient[] interfaces = _wireGuardClientFactory.Create(instance);
        WireGuardClient wg = interfaces.FirstOrDefault() ?? throw new("Failed to create WireGuard");

        // Act
        string output = _wgConfigFactory.Create(wg);

        // Assert
        await _context.Verify(output);
    }
}
