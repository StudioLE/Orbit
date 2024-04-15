using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using Orbit.Servers;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Serialization;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Servers;

internal sealed class ServerActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly ServerActivity _activity;
    private readonly ServerProvider _servers;
    private readonly ISerializer _serializer;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public ServerActivityTests()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _servers = provider.GetRequiredService<ServerProvider>();
        _activity = provider.GetRequiredService<ServerActivity>();
        _serializer = provider.GetRequiredService<ISerializer>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task ServerActivity_Execute_Default()
    {
        // Arrange
        Server sourceServer = new()
        {
            Name = new("new-server")
        };

        // Act
        Server createdServer = await _activity.Execute(sourceServer);

        // Assert
        Assert.That(createdServer, Is.Not.Null);
        await _context.VerifyAsSerialized(createdServer, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created server {createdServer.Name}"));
        Server storedServer = _servers.Get(createdServer.Name) ?? throw new("Failed to get server.");
        await _context.VerifyAsSerialized(storedServer, createdServer, _serializer);
    }
}
