using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using StudioLE.Orbit.Core.Tests.Resources;
using StudioLE.Orbit.Instances;
using StudioLE.Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Serialization;
using StudioLE.Verify.Serialization;

namespace StudioLE.Orbit.Core.Tests.Instances;

internal sealed class InstanceActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private InstanceActivity _activity = null!;
    private InstanceProvider _instances = null!;
    private ISerializer _serializer = null!;
    private IReadOnlyCollection<LogEntry> _logs = null!;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _instances = provider.GetRequiredService<InstanceProvider>();
        _activity = provider.GetRequiredService<InstanceActivity>();
        _serializer = provider.GetRequiredService<ISerializer>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task InstanceActivity_Execute_Default()
    {
        // Arrange
        Instance sourceInstance = new()
        {
            Server = new(MockConstants.ServerName)
        };

        // Act
        InstanceActivity.Outputs? outputs = await _activity.Execute(sourceInstance);

        // Assert
        Assert.That(outputs, Is.Not.Null);
        Assert.That(outputs!.Status.ExitCode, Is.EqualTo(0), "ExitCode");
        await _context.VerifyAsSerialized(outputs.Instance, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created instance {outputs.Instance.Name}"));
        Instance storedInstance = await _instances.Get(outputs.Instance.Name) ?? throw new("Failed to get instance.");
        await _context.VerifyAsSerialized(storedInstance, outputs.Instance, _serializer);
    }
}
