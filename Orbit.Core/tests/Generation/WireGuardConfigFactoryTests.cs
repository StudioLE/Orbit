using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Creation;
using Orbit.Core.Generation;
using Orbit.Core.Schema;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class WireGuardConfigFactoryTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly WireGuardFactory _wireGuardFactory;
    private readonly WireGuardConfigFactory _wireGuardConfigFactory;

    public WireGuardConfigFactoryTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _wireGuardFactory = host.Services.GetRequiredService<WireGuardFactory>();
        _wireGuardConfigFactory = host.Services.GetRequiredService<WireGuardConfigFactory>();
    }

    [Test]
    [Category("Factory")]
    public async Task WireGuardConfigFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        WireGuard[] interfaces = _wireGuardFactory.Create(instance);
        WireGuard wg = interfaces.FirstOrDefault() ?? throw new("Failed to create WireGuard");

        // Act
        string output = _wireGuardConfigFactory.Create(wg);

        // Assert
        await _verify.String(output);
    }
}
