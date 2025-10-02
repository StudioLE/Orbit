using System.Text.Json.Serialization;
using StudioLE.Extensions.System;

namespace StudioLE.Orbit.Utils.Networking;

/// <summary>
/// A MAC address.
/// </summary>
[JsonConverter(typeof(MacAddressJsonConverter))]
// ReSharper disable once InconsistentNaming
public readonly struct MacAddress
{
    /// <summary>
    /// The octets of the MAC address.
    /// </summary>
    public byte[] Octets { get; }

    /// <summary>
    /// Create a default instance of <see cref="MacAddress"/>.
    /// </summary>
    public MacAddress()
    {
        Octets = [0, 0, 0, 0, 0, 0];
    }

    /// <summary>
    /// Create a new instance of <see cref="MacAddress"/>.
    /// </summary>
    public MacAddress(byte[] octets)
    {
        if (octets.Length != 6)
            throw new ArgumentException("A MAC address must have a 6 octets.");
        Octets = octets;
    }

    /// <summary>
    /// Create a new instance of <see cref="MacAddress"/> by parsing a string.
    /// </summary>
    /// <remarks>
    /// The string is parsed with <see cref="MacAddressParser.Parse"/>.
    /// </remarks>
    public MacAddress(string macAddress)
    {
        MacAddress parsed = MacAddressParser.Parse(macAddress) ?? throw new ArgumentException("Invalid MAC address.", nameof(macAddress));
        Octets = parsed.Octets;
    }

    /// <summary>
    /// Is this a default value?
    /// </summary>
    public bool IsDefault()
    {
        return Equals(new MacAddress());
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Octets
            .Select(x => x.ToString("X2"))
            .Join(":");
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is not MacAddress macAddress)
            return false;
        return macAddress.Octets.SequenceEqual(Octets);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Octets);
    }
}
