using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using Orbit.Core.Utils.Serialization;
using StudioLE.Core.Serialization;
using StudioLE.Verify;
using StudioLE.Verify.Json;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests;

internal sealed class SerializationTests
{
    private const string Source = $"""
        Name: instance-01
        Number: 1
        Role: node
        Server: server-01
        Networks:
        - network-01
        Domains: []
        OS:
          Name: ubuntu
          Version: jammy
        Hardware:
          Platform: Virtual
          Type: G1
          Cpus: 1
          Memory: 4
          Disk: 20
        WireGuard:
        - Name: wg1
          Network: network-01
          PrivateKey: {MockWireGuardFacade.PrivateKey}
          PublicKey: {MockWireGuardFacade.PublicKey}
          Addresses:
          - 10.1.1.0
          - 'fc00:1:1::'
          ServerPublicKey: ''
          AllowedIPs:
          - 0.0.0.0/0
          - ::/0
          Endpoint: localhost:51820
        Mounts:
        - Source: /mnt/instance-01/srv
          Target: /srv
        - Source: /mnt/instance-01/config
          Target: /config
        Repo:
        """;
    private readonly IVerify _verify = new NUnitVerify();
    private readonly InstanceFactory _instanceFactory;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public SerializationTests()
    {
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
        _deserializer = host.Services.GetRequiredService<IDeserializer>();
    }

    [Test]
    public async Task Instance_Serialize()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(TestHelpers.ExampleInstance());

        // Act
        string serialized = _serializer.Serialize(instance);

        // Assert
        await _verify.String(serialized);
    }

    [Test]
    public async Task Instance_Deserialize()
    {
        // Arrange
        // Act
        Instance instance = _deserializer.Deserialize<Instance>(Source) ?? throw new("Failed to deserialize.");

        // Assert
        await _verify.AsJson(instance);
    }

    [Test]
    public async Task Instance_Serialization_RoundTrip()
    {
        // Arrange
        // Act
        Instance instance = _deserializer.Deserialize<Instance>(Source) ?? throw new("Failed to deserialize.");
        string serialized = _serializer.Serialize(instance);

        // Assert
        const string expected = Source + " ";
        await _verify.String(expected, serialized);
    }
}
