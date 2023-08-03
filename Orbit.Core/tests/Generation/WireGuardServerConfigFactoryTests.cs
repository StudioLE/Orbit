using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class WireGuardServerConfigFactoryTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly IEntityProvider<Network> _networks;
    private readonly WireGuardServerConfigFactory _wgConfigFactory;

    public WireGuardServerConfigFactoryTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _networks = host.Services.GetRequiredService<IEntityProvider<Network>>();
        _wgConfigFactory = host.Services.GetRequiredService<WireGuardServerConfigFactory>();
    }

    [Test]
    [Category("Factory")]
    public async Task WireGuardServerConfigFactory_Create()
    {
        // Arrange
        Network network = _networks.GetIndex()
                              .Select(x => _networks.Get(new NetworkId(x)))
                              .FirstOrDefault()
                          ?? throw new("Failed to get Network");

        // Act
        string output = _wgConfigFactory.Create(network);

        // Assert
        await _verify.String(output);
    }
}
