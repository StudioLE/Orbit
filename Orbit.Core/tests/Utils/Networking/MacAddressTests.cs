using System.Text.Json;
using NUnit.Framework;
using Orbit.Utils.Networking;
using YamlDotNet.Serialization;

namespace Orbit.Core.Tests.Utils.Networking;

// ReSharper disable once InconsistentNaming
internal sealed class MacAddressTests
{
    [TestCase(0, 0, 0, 0, 0, 0, "00:00:00:00:00:00")]
    [TestCase(0, 12, 34, 56, 78, 90, "00:0C:22:38:4E:5A")]
    [TestCase(255, 255, 255, 255, 255, 255, "FF:FF:FF:FF:FF:FF")]
    [Category("Misc")]
    public void MacAddress_Constructor_Bytes(byte a, byte b, byte c, byte d, byte e, byte f, string str)
    {
        // Arrange
        // Act
        MacAddress macAddress = new([a, b, c, d, e, f]);

        // Assert
        Assert.That(macAddress.ToString(), Is.EqualTo(str));
    }

    [TestCase(0, 0, 0, 0, 0, 0, "00:00:00:00:00:00")]
    [TestCase(0, 12, 34, 56, 78, 90, "00:0C:22:38:4E:5A")]
    [TestCase(255, 255, 255, 255, 255, 255, "FF:FF:FF:FF:FF:FF")]
    [Category("Misc")]
    public void MacAddress_Constructor_String(byte a, byte b, byte c, byte d, byte e, byte f, string str)
    {
        // Arrange
        MacAddress expected = new([a, b, c, d, e, f]);

        // Act
        MacAddress actual = new(str);

        // Assert
        Assert.That(actual.ToString(), Is.EqualTo(str));
        Assert.That(actual.ToString(), Is.EqualTo(expected.ToString()));
        Assert.That(actual.Equals(expected));
    }

    [TestCase("AA:BB:CC:DD:EE:FG")]
    [TestCase("AA:BB:CC:DD:EE")]
    [TestCase("")]
    [Category("Misc")]
    public void MacAddress_Constructor_String_Throws(string str)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => _ = new MacAddress(str));
    }

    [TestCase("AA:BB:CC:DD:EE:FF")]
    [Category("Misc")]
    public void MacAddress_Serialization_Json(string source)
    {
        // Arrange
        MacAddress macAddress = new(source);

        // Act
        string serialized = JsonSerializer.Serialize(macAddress);
        MacAddress? deserialized = JsonSerializer.Deserialize<MacAddress>(serialized);

        // Assert
        Assert.That(serialized, Is.EqualTo('"' + source + '"'));
        Assert.That(deserialized, Is.EqualTo(macAddress));
    }

    [TestCase("AA:BB:CC:DD:EE:FF")]
    [Category("Misc")]
    public void MacAddress_Serialization_Yaml(string source)
    {
        // Arrange
        MacAddressYamlConverter converter = new();
        ISerializer serializer = new SerializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        IDeserializer deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        MacAddress macAddress = new(source);

        // Act
        string serialized = serializer.Serialize(macAddress);
        MacAddress? deserialized = deserializer.Deserialize<MacAddress>(serialized);

        // Assert
        Assert.That(serialized, Is.EqualTo(source + Environment.NewLine));
        Assert.That(deserialized, Is.EqualTo(macAddress));
    }
}
