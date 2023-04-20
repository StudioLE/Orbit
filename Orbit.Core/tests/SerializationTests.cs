using Orbit.Core.Providers;
using Orbit.Core.Schema;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests;

internal sealed class SerializationTests
{
    private readonly EntityProvider _provider = EntityProvider.CreateTemp();
    private readonly Verify _verify = new(new NUnitVerifyContext());
    private const string SourceYaml = """
        name: cluster-01-01
        number: 1
        role: node
        cluster:
          name: cluster-01
          number: 1
        host:
          name: host-01
          number: 1
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
        """;

    [Test]
    public async Task Instance_Serialize()
    {
        // Arrange
        Instance instance = new();
        instance.Review(_provider);

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
