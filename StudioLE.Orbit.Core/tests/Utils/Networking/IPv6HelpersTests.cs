using NUnit.Framework;
using StudioLE.Orbit.Utils.Networking;

namespace StudioLE.Orbit.Core.Tests.Utils.Networking;

// ReSharper disable once InconsistentNaming
internal sealed class IPv6HelpersTests
{
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, null, "10:f0ff:0:1:0:0:0:0")]
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, 24, "10:f0ff:0:1:0:0:0:0/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, null, "0:0:cafe:0:0:ace:0:0")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, 24, "0:0:cafe:0:0:ace:0:0/24")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, null, "0:bee:cafe:0:0:ace:abba:0")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, 24, "0:bee:cafe:0:0:ace:abba:0/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, null, "0:0:0:200:30:f111:2:3")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, 24, "0:0:0:200:30:f111:2:3/24")]
    [TestCase(new ushort[] { }, 0, "0:0:0:0:0:0:0:0/0")]
    [Category("Misc")]
    public void IPv6Helpers_ToFullString(ushort[] hextets, byte? cidr, string expected)
    {
        // Arrange
        IPv6 ip = new(hextets, cidr);

        // Act
        string actual = ip.ToFullString();

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, null, "10:f0ff:0:1:0:0:0:0")]
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, 24, "10:f0ff:0:1:0:0:0:0/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, null, "::cafe:0:0:ace:0:0")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, 24, "::cafe:0:0:ace:0:0/24")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, null, "::bee:cafe:0:0:ace:abba:0")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, 24, "::bee:cafe:0:0:ace:abba:0/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, null, "::200:30:f111:2:3")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, 24, "::200:30:f111:2:3/24")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, null, "1:2:3:4:5:6:7:8")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, 24, "1:2:3:4:5:6:7:8/24")]
    [TestCase(new ushort[] { }, 0, "::/0")]
    [Category("Misc")]
    public void IPv6Helpers_ShortenFromStart(ushort[] hextets, byte? cidr, string expected)
    {
        // Arrange
        IPv6 ip = new(hextets, cidr);

        // Act
        string actual = ip.ShortenFromStart();

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, null, "10:f0ff:0:1::")]
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, 24, "10:f0ff:0:1::/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, null, "0:0:cafe:0:0:ace::")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, 24, "0:0:cafe:0:0:ace::/24")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, null, "0:bee:cafe:0:0:ace:abba::")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, 24, "0:bee:cafe:0:0:ace:abba::/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, null, "0:0:0:200:30:f111:2:3")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, 24, "0:0:0:200:30:f111:2:3/24")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, null, "1:2:3:4:5:6:7:8")]
    [TestCase(new ushort[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8 }, 24, "1:2:3:4:5:6:7:8/24")]
    [TestCase(new ushort[] { }, 0, "::/0")]
    [Category("Misc")]
    public void IPv6Helpers_ShortenFromEnd(ushort[] hextets, byte? cidr, string expected)
    {
        // Arrange
        IPv6 ip = new(hextets, cidr);

        // Act
        string actual = ip.ShortenFromEnd();

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
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
    public void IPv6Helpers_Shorten(ushort[] hextets, byte? cidr, string expected)
    {
        // Arrange
        IPv6 ip = new(hextets, cidr);

        // Act
        string actual = ip.Shorten();

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, null, "0010:f0ff:0000:0001::")]
    [TestCase(new ushort[] { 0x10, 0xf0ff, 0x0, 0x1 }, 24, "0010:f0ff:0000:0001::/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, null, "::cafe:0000:0000:0ace:0000:0000")]
    [TestCase(new ushort[] { 0x0, 0x0, 0xcafe, 0x0, 0x0, 0x0ace, 0x0, 0x0 }, 24, "::cafe:0000:0000:0ace:0000:0000/24")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, null, "0000:0bee:cafe::0ace:abba:0000")]
    [TestCase(new ushort[] { 0x0, 0xbee, 0xcafe, 0x0, 0x0, 0x0ace, 0xabba, 0x0 }, 24, "0000:0bee:cafe::0ace:abba:0000/24")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, null, "::0200:0030:f111:0002:0003")]
    [TestCase(new ushort[] { 0x0, 0x0, 0x0, 0x200, 0x30, 0xf111, 0x2, 0x3 }, 24, "::0200:0030:f111:0002:0003/24")]
    [TestCase(new ushort[] { }, 0, "::/0")]
    [Category("Misc")]
    public void IPv6Helpers_Shorten_Expanded(ushort[] hextets, byte? cidr, string expected)
    {
        // Arrange
        IPv6 ip = new(hextets, cidr);

        // Act
        string actual = ip.Shorten(false);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCase("cafe", (ushort)0xcafe)]
    [TestCase("a", (ushort)0xa)]
    [TestCase("10", (ushort)0x10)]
    [Category("Misc")]
    public void IPv6Helpers_ToHextet(string str, ushort expected)
    {
        // Arrange
        // Act
        int integer = Convert.ToInt32(str, 16);
        ushort actual = (ushort)integer;

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }
}
