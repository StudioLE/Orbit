using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Creation;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Serialization;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Creation;

internal sealed class CreateNetworkTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly CreateNetwork _activity;
    private readonly IEntityProvider<Network> _networks;
    private readonly IEntityProvider<Server> _servers;
    private readonly ISerializer _serializer;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public CreateNetworkTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _activity = host.Services.GetRequiredService<CreateNetwork>();
        _networks = host.Services.GetRequiredService<IEntityProvider<Network>>();
        _servers = host.Services.GetRequiredService<IEntityProvider<Server>>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
        _logs = host.Services.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task CreateNetwork_Execute_Default()
    {
        // Arrange
        Network sourceNetwork = new()
        {
            Name = "new-network",
            Server = MockConstants.ServerName
        };

        // Act
        Network? createdNetwork = await _activity.Execute(sourceNetwork);

        // Assert
        if (createdNetwork is null)
            Assert.Fail();
        else
            await _verify.AsSerialized(createdNetwork, _serializer);
        Assert.That(_logs.Count, Is.EqualTo(1));
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Created network {createdNetwork!.Name}"));
        Network storedNetwork = _networks.Get(new NetworkId(createdNetwork.Name)) ?? throw new("Failed to get network.");
        await _verify.AsSerialized(storedNetwork, createdNetwork, _serializer);
        string fileName = WireGuardServerConfigFactory.GetFileName(createdNetwork);
        string? wgConfig = _servers.GetResource(new ServerId(createdNetwork.Server), fileName);
        Assert.That(wgConfig, Is.Not.Null);
        await _verify.String(wgConfig!);
    }
}
