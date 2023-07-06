using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Activities;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Core.Serialization;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Activities;

internal sealed class CreateTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly Create _create;
    private readonly IEntityProvider<Instance> _instances;
    private readonly ISerializer _serializer;

    public CreateTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _create = host.Services.GetRequiredService<Create>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
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
            await _verify.AsSerialized(createdInstance, _serializer);
        Assert.That(logger.Logs.Count, Is.EqualTo(1));
        Assert.That(logger.Logs.ElementAt(0).Message, Is.EqualTo($"Created instance {createdInstance!.Name}"));
        Instance storedInstance = _instances.Get(new InstanceId(createdInstance.Name)) ?? throw new("Failed to get instance.");
        await _verify.AsSerialized(storedInstance, createdInstance, _serializer);
    }
}
