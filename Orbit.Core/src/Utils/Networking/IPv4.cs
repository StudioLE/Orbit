using System.Text.Json.Serialization;

namespace Orbit.Utils.Networking;

/// <summary>
/// An IPv4 address.
/// </summary>
[JsonConverter(typeof(IPv4JsonConverter))]
// ReSharper disable once InconsistentNaming
public readonly struct IPv4
{
    /// <summary>
    /// The four octets of the IPv4 address.
    /// </summary>
    public byte[] Octets { get; }

    /// <summary>
    /// The optional CIDR of the IPv4 address.
    /// </summary>
    public byte? Cidr { get; }

    /// <summary>
    /// Create a new instance of <see cref="IPv4"/>.
    /// </summary>
    public IPv4(byte[] octets, byte? cidr = null)
    {
        Octets = octets.Length != 4
            ? throw new ArgumentOutOfRangeException(nameof(octets), "IPv4 must have 4 octets.")
            : octets;
        Cidr = cidr is > 32
            ? throw new ArgumentOutOfRangeException(nameof(cidr), "IPv4 CIDR must be between 0 and 32.")
            : cidr;
    }

    /// <summary>
    /// Create a new instance of <see cref="IPv4"/>.
    /// </summary>
    public IPv4(byte a, byte b, byte c, byte d, byte? cidr = null) : this([a, b, c, d], cidr)
    {
    }

    /// <summary>
    /// Create a new instance of <see cref="IPv4"/> by parsing a string.
    /// </summary>
    /// <remarks>
    /// The string is parsed with <see cref="IPv4Parser.Parse"/>.
    /// </remarks>
    public IPv4(string ipv4)
    {
        IPv4 parsed = IPv4Parser.Parse(ipv4) ?? throw new ArgumentException("Invalid IPv4 address.", nameof(ipv4));
        Octets = parsed.Octets;
        Cidr = parsed.Cidr;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Cidr is null
            ? $"{Octets[0]}.{Octets[1]}.{Octets[2]}.{Octets[3]}"
            : $"{Octets[0]}.{Octets[1]}.{Octets[2]}.{Octets[3]}/{Cidr}";
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not IPv4 ip)
            return false;
        return ip.Cidr.Equals(Cidr)
               && ip.Octets.SequenceEqual(Octets);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Octets, Cidr);
    }
}
