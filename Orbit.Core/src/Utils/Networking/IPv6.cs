using System.Text.Json.Serialization;

namespace Orbit.Utils.Networking;

[JsonConverter(typeof(IPv6JsonConverter))]
// ReSharper disable once InconsistentNaming
public readonly struct IPv6
{
    internal readonly ushort[] _hextets;
    internal readonly byte? _cidr;

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
        _hextets = Array.Empty<ushort>()
            .Concat(preShortened)
            .Concat(Enumerable.Repeat<ushort>(0, shortenedCount))
            .Concat(postShortened)
            .ToArray();
        _cidr = cidr;
    }

    public IPv6(string ipv6)
    {
        IPv6 parsed = IPv6Parser.Parse(ipv6) ?? throw new ArgumentException("Invalid IPv4 address.", nameof(ipv6));
        _hextets = parsed._hextets;
        _cidr = parsed._cidr;
    }

    public string? GetCidr()
    {
        return _cidr.ToString();
    }

    public ushort[] GetHextets()
    {
        return _hextets;
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
        return ip._cidr.Equals(_cidr)
               && ip._hextets.SequenceEqual(_hextets);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(_hextets, _cidr);
    }
}
