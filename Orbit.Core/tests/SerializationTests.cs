using Orbit.Core.Schema;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using YamlDotNet.Serialization;

namespace Orbit.Core.Tests;

internal sealed class SerializationTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());
    private const string SourceYaml = """
            name: node-01-01
            id: 12-01
            number: 1
            cluster: 12
            host: 2
            role: node
            network:
              address: 10.1.1.1
              gateway: 10.0.0.1
            os:
              name: ubuntu
              version: jammy
            hardware:
              platform: Virtual
              type: G1
              cpus: 2
              memory: 8
              disk: 20
            """;

    [Test]
    public async Task Instance_Deserialize()
    {
        // Arrange
        IDeserializer deserializer = Yaml.Deserializer();

        // Act
        Instance instance = deserializer.Deserialize<Instance>(SourceYaml);

        // Assert
        await _verify.AsJson(instance);
    }

    [Test]
    public async Task Instance_Serialization_RoundTrip()
    {
        // Arrange
        IDeserializer deserializer = Yaml.Deserializer();
        ISerializer serializer = Yaml.Serializer();

        // Act
        Instance instance = deserializer.Deserialize<Instance>(SourceYaml);
        string serializedYaml = serializer.Serialize(instance);

        // Assert
        await _verify.String(SourceYaml, serializedYaml);
    }
}
