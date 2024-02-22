using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Creation.Clients;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Serialization;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Creation;

internal sealed class CreateClientTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CreateClient _activity;
    private readonly IEntityProvider<Client> _clients;
    private readonly ISerializer _serializer;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public CreateClientTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _activity = host.Services.GetRequiredService<CreateClient>();
        _clients = host.Services.GetRequiredService<IEntityProvider<Client>>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
        _logs = host.Services.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task CreateClient_Execute_Default()
    {
        // Arrange
        Client sourceClient = new()
        {
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
        Client? createdClient = await _activity.Execute(sourceClient);

        // Assert
        TestHelpers.UseMockMacAddress(createdClient!);
        Assert.That(createdClient, Is.Not.Null);
        await _context.VerifyAsSerialized(createdClient!, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created client {createdClient!.Name}"));
        Client storedClient = _clients.Get(new ClientId(createdClient.Name)) ?? throw new("Failed to get client.");
        TestHelpers.UseMockMacAddress(storedClient);
        await _context.VerifyAsSerialized(storedClient, createdClient, _serializer);
    }
}
