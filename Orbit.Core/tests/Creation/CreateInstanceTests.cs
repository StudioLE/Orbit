using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Creation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Core.Serialization;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Creation;

internal sealed class CreateInstanceTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly CreateInstance _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly ISerializer _serializer;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public CreateInstanceTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _activity = host.Services.GetRequiredService<CreateInstance>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
        _logs = host.Services.GetTestLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task CreateInstance_Execute_Default()
    {
        // Arrange
        Instance sourceInstance = TestHelpers.ExampleInstance();

        // Act
        Instance? createdInstance = await _activity.Execute(sourceInstance);

        // Assert
        if (createdInstance is null)
            Assert.Fail();
        else
            await _verify.AsSerialized(createdInstance, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created instance {createdInstance!.Name}"));
        Instance storedInstance = _instances.Get(new InstanceId(createdInstance.Name)) ?? throw new("Failed to get instance.");
        await _verify.AsSerialized(storedInstance, createdInstance, _serializer);
    }
}
