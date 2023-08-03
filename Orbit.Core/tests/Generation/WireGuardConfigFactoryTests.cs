using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Creation;
using Orbit.Core.Generation;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class WireGuardConfigFactoryTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly WireGuardClientFactory _wireGuardClientFactory;
    private readonly WireGuardClientConfigFactory _wgConfigFactory;

    public WireGuardConfigFactoryTests()
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
    public async Task WireGuardConfigFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        WireGuardClient[] interfaces = _wireGuardClientFactory.Create(instance);
        WireGuardClient wg = interfaces.FirstOrDefault() ?? throw new("Failed to create WireGuard");

        // Act
        string output = _wgConfigFactory.Create(wg);

        // Assert
        await _verify.String(output);
    }
}
