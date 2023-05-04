using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Schema;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests;

internal sealed class SerializationTests
{
    private const string SourceYaml = """
        name: cluster-01-01
        number: 1
        role: node
        cluster: cluster-01
        server: server-01
        network:
          address: 10.1.1.1
          gateway: 10.1.0.1
        os:
          name: ubuntu
          version: jammy
        hardware:
          platform: Virtual
          type: G1
          cpus: 1
          memory: 4
          disk: 20
        wireguard:
          private_key: 8Dh1P7/6fm9C/wHYzDrEhnyKmFgzL6yH6WuslXPLbVQ=
          public_key: Rc9kAH9gclSHur2vbbmIj3pvWizuxB5ly1Drv0tRXRE=
          addresses: []
          server_public_key: ''
          allowed_i_ps: ''
          endpoint: ''
        """;
    private readonly Verify _verify = new(new NUnitVerifyContext());
    private readonly InstanceFactory _instanceFactory;

    public SerializationTests()
    {
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
    }

    [Test]
    public async Task Instance_Serialize()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(new()
        {
            Server = "server-01",
            Cluster = "cluster-01",
            WireGuard =
            {
                PrivateKey = "8Dh1P7/6fm9C/wHYzDrEhnyKmFgzL6yH6WuslXPLbVQ=",
                PublicKey = "Rc9kAH9gclSHur2vbbmIj3pvWizuxB5ly1Drv0tRXRE="
            }
        });

        // Act
        string yaml = Yaml.Serialize(instance);

        // Assert
        await _verify.String(yaml);
    }

    [Test]
    public async Task Instance_Deserialize()
    {
        // Arrange
        // Act
        Instance instance = Yaml.Deserialize<Instance>(SourceYaml);

        // Assert
        await _verify.AsJson(instance);
    }

    [Test]
    public async Task Instance_Serialization_RoundTrip()
    {
        // Arrange
        // Act
        Instance instance = Yaml.Deserialize<Instance>(SourceYaml);
        string serializedYaml = Yaml.Serialize(instance);

        // Assert
        await _verify.String(SourceYaml, serializedYaml);
    }
}
