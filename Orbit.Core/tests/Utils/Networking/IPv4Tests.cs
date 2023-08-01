using System.Text.Json;
using NUnit.Framework;
using Orbit.Core.Utils.Networking;
using YamlDotNet.Serialization;

namespace Orbit.Core.Tests.Utils.Networking;

// ReSharper disable once InconsistentNaming
internal sealed class IPv4Tests
{
    [TestCase(10, 0, 0, 1, 24, "10.0.0.1/24")]
    [TestCase(10, 0, 0, 1, 32, "10.0.0.1/32")]
    [TestCase(10, 0, 0, 1, null, "10.0.0.1")]
    [Category("Misc")]
    public void IPv4_Constructor_Bytes(byte a, byte b, byte c, byte d, byte? cidr, string str)
    {
        // Arrange
        // Act
        IPv4 ip = new(a, b, c, d, cidr);

        // Assert
        Assert.That(ip.ToString(), Is.EqualTo(str));
    }

    [TestCase(10, 0, 0, 1, 33)]
    [Category("Misc")]
    public void IPv4_Constructor_Bytes_Throws(byte a, byte b, byte c, byte d, byte? cidr)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new IPv4(a, b, c, d, cidr));
    }

    [TestCase(10, 0, 0, 1, 24, "10.0.0.1/24")]
    [TestCase(10, 0, 0, 1, 32, "10.0.0.1/32")]
    [TestCase(10, 0, 0, 1, null, "10.0.0.1")]
    [Category("Misc")]
    public void IPv4_Constructor_String(byte a, byte b, byte c, byte d, byte? cidr, string str)
    {
        // Arrange
        IPv4 expected = new(a, b, c, d, cidr);

        // Act
        IPv4 actual = new(str);

        // Assert
        Assert.That(actual.ToString(), Is.EqualTo(str));
        Assert.That(actual.ToString(), Is.EqualTo(expected.ToString()));
        Assert.That(actual.Equals(expected));
    }

    [TestCase("10.0.0.1/33")]
    [TestCase("10.0.0.1/3/2")]
    [TestCase("10.0.0/32")]
    [TestCase("10.0.0.1.1/32")]
    [TestCase("256.0.0.1/32")]
    [Category("Misc")]
    public void IPv4_Constructor_String_Throws(string str)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => _ = new IPv4(str));
    }

    [TestCase("10.0.0.1/24")]
    [TestCase("10.0.0.1/32")]
    [TestCase("10.0.0.1")]
    [Category("Misc")]
    public void IPv4_Serialization_Json(string source)
    {
        // Arrange
        IPv4 ip = new(source);

        // Act
        string serialized = JsonSerializer.Serialize(ip);
        IPv4? deserialized = JsonSerializer.Deserialize<IPv4>(serialized);

        // Assert
        Assert.That(serialized, Is.EqualTo('"' + source + '"'));
        Assert.That(deserialized, Is.EqualTo(ip));
    }

    [TestCase("10.0.0.1/24")]
    [TestCase("10.0.0.1/32")]
    [TestCase("10.0.0.1")]
    [Category("Misc")]
    public void IPv4_Serialization_Yaml(string source)
    {
        // Arrange
        IPv4YamlConverter converter = new();
        ISerializer serializer = new SerializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        IDeserializer deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        IPv4 ip = new(source);

        // Act
        string serialized = serializer.Serialize(ip);
        IPv4? deserialized = deserializer.Deserialize<IPv4>(serialized);

        // Assert
        Assert.That(serialized, Is.EqualTo(source + Environment.NewLine));
        Assert.That(deserialized, Is.EqualTo(ip));
    }
}
