using System.Text.Json.Serialization;

namespace Orbit.Utils.Networking;

/// <summary>
/// An IPv6 address.
/// </summary>
[JsonConverter(typeof(IPv6JsonConverter))]
// ReSharper disable once InconsistentNaming
public readonly struct IPv6
{
    /// <summary>
    /// The hextets of the IPv6 address.
    /// </summary>
    public ushort[] Hextets { get; }

    /// <summary>
    /// The CIDR of the IPv6 address.
    /// </summary>
    public byte? Cidr { get; }

    /// <summary>
    /// Create a new instance of <see cref="IPv6"/>.
    /// </summary>
    public IPv6(ushort[] hextets, byte? cidr = null) : this(hextets, Array.Empty<ushort>(), cidr)
    {
    }

    internal IPv6(ushort[] preShortened, ushort[] postShortened, byte? cidr = null)
    {
        int preCount = preShortened.Length;
        int postCount = postShortened.Length;
        int shortenedCount = 8 - preCount - postCount;
        if (preCount + postCount > 8)
            throw new ArgumentException("IPv6 must have a maximum of 8 hextets.");
        if (cidr is > 128)
            throw new ArgumentOutOfRangeException(nameof(cidr), "CIDR must be between 0 and 128.");
        Hextets = Array.Empty<ushort>()
            .Concat(preShortened)
            .Concat(Enumerable.Repeat<ushort>(0, shortenedCount))
            .Concat(postShortened)
            .ToArray();
        Cidr = cidr;
    }

    /// <summary>
    /// Create a new instance of <see cref="IPv6"/> by parsing a string.
    /// </summary>
    /// <remarks>
    /// The string is parsed with <see cref="IPv6Parser.Parse"/>.
    /// </remarks>
    public IPv6(string ipv6)
    {
        IPv6 parsed = IPv6Parser.Parse(ipv6) ?? throw new ArgumentException("Invalid IPv4 address.", nameof(ipv6));
        Hextets = parsed.Hextets;
        Cidr = parsed.Cidr;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.Shorten();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not IPv6 ip)
            return false;
        return ip.Cidr.Equals(Cidr)
               && ip.Hextets.SequenceEqual(Hextets);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Hextets, Cidr);
    }
}
