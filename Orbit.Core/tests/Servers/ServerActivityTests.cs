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
    private ServerActivity _activity;
    private ServerProvider _servers;
    private ISerializer _serializer;
    private IReadOnlyCollection<LogEntry> _logs;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
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
        ServerActivity.Outputs? outputs = await _activity.Execute(sourceServer);

        // Assert
        Assert.That(outputs, Is.Not.Null);
        Assert.That(outputs!.Status.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(outputs.Server, Is.Not.Null);
        await _context.VerifyAsSerialized(outputs.Server, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created server {outputs.Server.Name}"));
        Server storedServer = await _servers.Get(outputs.Server.Name) ?? throw new("Failed to get server.");
        await _context.VerifyAsSerialized(storedServer, outputs.Server, _serializer);
    }
}
