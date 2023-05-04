using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Activities;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests;

internal sealed class CreateTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());
    private readonly Create _create;
    private readonly EntityProvider _provider;

    public CreateTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _create = host.Services.GetRequiredService<Create>();
        _provider = host.Services.GetRequiredService<EntityProvider>();
    }

    [Test]
    public async Task Create_Execute_Default()
    {
        // Arrange
        TestLogger logger = TestLogger.GetInstance();
        Instance sourceInstance = new()
        {
            WireGuard =
            {
                PrivateKey = MockWireGuardFacade.PrivateKey,
                PublicKey = MockWireGuardFacade.PublicKey
            }
        };

        // Act
        Instance? createdInstance = await _create.Execute(sourceInstance);

        // Assert
        if (createdInstance is null)
            Assert.Fail();
        else
            await _verify.AsYaml(createdInstance);

        Assert.That(logger.Logs.Count, Is.EqualTo(1));
        Assert.That(logger.Logs.ElementAt(0).Message, Is.EqualTo($"Created instance {createdInstance!.Name}"));
        Instance storedInstance = _provider.Instance.Get(createdInstance.Cluster, createdInstance.Name) ?? throw new("Failed to get instance.");
        await _verify.AsYaml(storedInstance, createdInstance);
        string? networkConfig = _provider.Instance.GetResource(createdInstance.Cluster, createdInstance.Name, "network-config.yml");
        string? userConfig = _provider.Instance.GetResource(createdInstance.Cluster, createdInstance.Name, "user-config.yml");
        Assert.That(networkConfig, Is.Not.Null);
        Assert.That(userConfig, Is.Not.Null);
    }
}
