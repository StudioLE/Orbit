using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Clients;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Serialization;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Clients;

internal sealed class ClientActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private ClientActivity _activity = null!;
    private ClientProvider _clients = null!;
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
        _clients = provider.GetRequiredService<ClientProvider>();
        _activity = provider.GetRequiredService<ClientActivity>();
        _serializer = provider.GetRequiredService<ISerializer>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task ClientActivity_Execute_Default()
    {
        // Arrange
        Client sourceClient = new();

        // Act
        ClientActivity.Outputs? outputs = await _activity.Execute(sourceClient);

        // Assert
        Assert.That(outputs, Is.Not.Null);
        Assert.That(outputs!.Status.ExitCode, Is.EqualTo(0));
        await _context.VerifyAsSerialized(outputs.Client, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created client {outputs.Client.Name}"));
        Client storedClient = await _clients.Get(outputs.Client.Name) ?? throw new("Failed to get client.");
        await _context.VerifyAsSerialized(storedClient, outputs.Client, _serializer);
    }
}
