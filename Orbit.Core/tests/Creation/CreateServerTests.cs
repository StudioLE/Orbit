using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Creation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Core.Serialization;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Creation;

internal sealed class CreateServerTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly CreateServer _activity;
    private readonly IEntityProvider<Server> _servers;
    private readonly ISerializer _serializer;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public CreateServerTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _activity = host.Services.GetRequiredService<CreateServer>();
        _servers = host.Services.GetRequiredService<IEntityProvider<Server>>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
        _logs = host.Services.GetTestLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task CreateServer_Execute_Default()
    {
        // Arrange
        Server sourceServer = new()
        {
            Name = "new-server"
        };

        // Act
        Server? createdServer = await _activity.Execute(sourceServer);

        // Assert
        if (createdServer is null)
            Assert.Fail();
        else
            await _verify.AsSerialized(createdServer, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created server {createdServer!.Name}"));
        Server storedServer = _servers.Get(new ServerId(createdServer.Name)) ?? throw new("Failed to get server.");
        await _verify.AsSerialized(storedServer, createdServer, _serializer);
    }
}
