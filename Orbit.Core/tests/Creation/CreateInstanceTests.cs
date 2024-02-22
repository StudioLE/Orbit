using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Creation.Instances;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Serialization;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Creation;

internal sealed class CreateInstanceTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CreateInstance _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly ISerializer _serializer;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public CreateInstanceTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _instances = provider.GetRequiredService<IEntityProvider<Instance>>();
        _activity = provider.GetRequiredService<CreateInstance>();
        _serializer = provider.GetRequiredService<ISerializer>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task CreateInstance_Execute_Default()
    {
        // Arrange
        Instance sourceInstance = new()
        {
            Server = MockConstants.ServerName,
            WireGuard =
            [
                new()
                {
                    PrivateKey = MockConstants.PrivateKey,
                    PublicKey = MockConstants.PublicKey,
                    PreSharedKey = MockConstants.PreSharedKey
                }
            ]
        };

        // Act
        Instance? createdInstance = await _activity.Execute(sourceInstance);

        // Assert
        Assert.That(createdInstance, Is.Not.Null);
        TestHelpers.UseMockMacAddress(createdInstance!);
        await _context.VerifyAsSerialized(createdInstance!, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created instance {createdInstance!.Name}"));
        Instance storedInstance = _instances.Get(new InstanceId(createdInstance.Name)) ?? throw new("Failed to get instance.");
        TestHelpers.UseMockMacAddress(storedInstance);
        await _context.VerifyAsSerialized(storedInstance, createdInstance, _serializer);
    }
}
