using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using Orbit.Utils.Serialization;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Serialization;
using StudioLE.Verify;

namespace Orbit.Core.Tests;

internal sealed class SerializationTests
{
    private readonly IContext _context = new NUnitContext();
    private ISerializer _serializer;
    private IDeserializer _deserializer;

    [SetUp]
    public async Task SetUp()
    {
        IHost host = await TestHelpers.CreateTestHost();
        _serializer = host.Services.GetRequiredService<ISerializer>();
        _deserializer = host.Services.GetRequiredService<IDeserializer>();
    }

    [Test]
    [Category("Serialization")]
    public async Task Instance_Serialize()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();

        // Act
        string serialized = _serializer.Serialize(instance);

        // Assert
        await _context.Verify(serialized);
    }

    [Test]
    [Category("Serialization")]
    public async Task Instance_Deserialize()
    {
        // Arrange
        string path = "../../../Verify/SerializationTests.Instance_Serialize.verified.txt";
        if (!File.Exists(path))
            throw new("The file does not exist.");
        string yaml = await File.ReadAllTextAsync(path);

        // Act
        Instance instance = _deserializer.Deserialize<Instance>(yaml) ?? throw new("Failed to deserialize.");
        JsonSerializerOptions options = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new ParseableJsonConverter()
            },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(instance, options);

        // Assert
        await _context.Verify(json);
    }

    [Test]
    [Category("Serialization")]
    public async Task Instance_Serialization_RoundTrip()
    {
        // Arrange
        string path = "../../../Verify/SerializationTests.Instance_Serialize.verified.txt";
        if (!File.Exists(path))
            throw new("The file does not exist.");
        string yaml = await File.ReadAllTextAsync(path);

        // Act
        Instance instance = _deserializer.Deserialize<Instance>(yaml) ?? throw new("Failed to deserialize.");
        string serialized = _serializer.Serialize(instance);

        // Assert
        string expected = yaml.Replace("Repo:", "Repo: ");
        await _context.Verify(expected, serialized);
    }
}
