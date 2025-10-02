using System.Text.Json;
using NUnit.Framework;
using StudioLE.Orbit.Utils.Networking;
using YamlDotNet.Serialization;

namespace StudioLE.Orbit.Core.Tests.Utils.Networking;

// ReSharper disable once InconsistentNaming
internal sealed class IPv6Tests
{
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, null, "10:f0ff:0:1::")]
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, 24, "10:f0ff:0:1::/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, null, "::cafe:0:0:ace:0:0")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, 24, "::cafe:0:0:ace:0:0/24")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, null, "0:bee:cafe::ace:abba:0")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, 24, "0:bee:cafe::ace:abba:0/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, null, "::200:30:f111:2:3")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, 24, "::200:30:f111:2:3/24")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, null, "1:2:3:4:5:6:7:8")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, 24, "1:2:3:4:5:6:7:8/24")]
    [TestCase(new ushort[] { }, 0, "::/0")]
    [Category("Misc")]
    public void IPv6_Constructor_Hex(ushort[] hextets, byte? cidr, string str)
    {
        // Arrange
        // Act
        IPv6 ip = new(hextets, cidr);

        // Assert
        Assert.That(ip.ToString(), Is.EqualTo(str));
    }

    [TestCase(new ushort[] { 0x10, 0x0, 0x0, 0x1 }, 129)]
    [Category("Misc")]
    public void IPv6_Constructor_Hex_Throws(ushort[] hextets, byte? cidr)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new IPv6(hextets, cidr));
    }

    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, null, "10:f0ff:0:1::")]
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, 24, "10:f0ff:0:1::/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, null, "::cafe:0:0:ace:0:0")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, 24, "::cafe:0:0:ace:0:0/24")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, null, "0:bee:cafe::ace:abba:0")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, 24, "0:bee:cafe::ace:abba:0/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, null, "::200:30:f111:2:3")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, 24, "::200:30:f111:2:3/24")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, null, "1:2:3:4:5:6:7:8")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, 24, "1:2:3:4:5:6:7:8/24")]
    [TestCase(new ushort[] { }, 0, "::/0")]
    [Category("Misc")]
    public void IPv6_Constructor_String(ushort[] hextets, byte? cidr, string str)
    {
        // Arrange
        IPv6 expected = new(hextets, cidr);

        // Act
        IPv6 actual = new(str);

        // Assert
        Assert.That(actual.ToString(), Is.EqualTo(str));
        Assert.That(actual.ToString(), Is.EqualTo(expected.ToString()));
        Assert.That(actual.Equals(expected));
    }

    [TestCase("10:0:0:1::/129")]
    [TestCase("10:0:0:1/3/2")]
    [TestCase("10:0:0:1/32")]
    [TestCase("1:2:3:4:5:6:7:8:9/32")]
    [TestCase("ff:race::/32")]
    [TestCase("1:2:3:4:5::6:7:8:9/32")]
    [Category("Misc")]
    public void IPv6_Constructor_String_Throws(string str)
    {
        // Arrange
        // Act
        // Assert
        Assert.Throws<ArgumentException>(() => _ = new IPv6(str));
    }

    [TestCase("10:f0ff:0:1::")]
    [TestCase("10:f0ff:0:1::/24")]
    [TestCase("::cafe:0:0:ace:0:0")]
    [TestCase("::cafe:0:0:ace:0:0/24")]
    [TestCase("0:bee:cafe::ace:abba:0")]
    [TestCase("0:bee:cafe::ace:abba:0/24")]
    [TestCase("::200:30:f111:2:3")]
    [TestCase("::200:30:f111:2:3/24")]
    [TestCase("1:2:3:4:5:6:7:8")]
    [TestCase("1:2:3:4:5:6:7:8/24")]
    [TestCase("::/0")]
    [Category("Misc")]
    public void IPv6_Serialization_Json(string source)
    {
        // Arrange
        IPv6 ip = new(source);

        // Act
        string serialized = JsonSerializer.Serialize(ip);
        IPv6? deserialized = JsonSerializer.Deserialize<IPv6>(serialized);

        // Assert
        Assert.That(serialized, Is.EqualTo('"' + source + '"'));
        Assert.That(deserialized, Is.EqualTo(ip));
    }

    [TestCase("10:f0ff:0:1::")]
    [TestCase("10:f0ff:0:1::/24")]
    [TestCase("::cafe:0:0:ace:0:0")]
    [TestCase("::cafe:0:0:ace:0:0/24")]
    [TestCase("0:bee:cafe::ace:abba:0")]
    [TestCase("0:bee:cafe::ace:abba:0/24")]
    [TestCase("::200:30:f111:2:3")]
    [TestCase("::200:30:f111:2:3/24")]
    [TestCase("1:2:3:4:5:6:7:8")]
    [TestCase("1:2:3:4:5:6:7:8/24")]
    [TestCase("::/0")]
    [Category("Misc")]
    public void IPv6_Serialization_Yaml(string source)
    {
        // Arrange
        IPv6YamlConverter converter = new();
        ISerializer serializer = new SerializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        IDeserializer deserializer = new DeserializerBuilder()
            .WithTypeConverter(converter)
            .Build();
        IPv6 ip = new(source);

        // Act
        string serialized = serializer.Serialize(ip);
        IPv6? deserialized = deserializer.Deserialize<IPv6>(serialized);

        // Assert
        string expected = serialized.StartsWith("'")
            ? $"'{source}'{Environment.NewLine}"
            : source + Environment.NewLine;
        Assert.That(serialized, Is.EqualTo(expected));
        Assert.That(deserialized, Is.EqualTo(ip));
    }
}
