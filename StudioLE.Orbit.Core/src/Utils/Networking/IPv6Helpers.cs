using StudioLE.Extensions.System;

namespace StudioLE.Orbit.Utils.Networking;

/// <summary>
/// Methods to help with <see cref="IPv6"/>.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IPv6Helpers
{
    /// <summary>
    /// Get the string representation of the IPv6 address with every octet.
    /// </summary>
    /// <param name="ip">The IPv6 address.</param>
    /// <param name="condensed">Should `0` values be condensed?</param>
    /// <returns>The string representation of the IPv6 address with every octet.</returns>
    public static string ToFullString(this IPv6 ip, bool condensed = true)
    {
        return ip.Hextets
                   .Select(x => x.ToHexString(condensed))
                   .Join(":")
               + ip.GetCidrSuffix();
    }

    /// <summary>
    /// Get the string representation of the IPv6 address shortened from the start.
    /// </summary>
    /// <param name="ip">The IPv6 address.</param>
    /// <param name="condensed">Should `0` values be condensed?</param>
    /// <returns>The string representation of the IPv6 address shortened from the start.</returns>
    public static string ShortenFromStart(this IPv6 ip, bool condensed = true)
    {
        int zerosAtStart = ip.Hextets
            .TakeWhile(x => x == 0)
            .Count();
        if (zerosAtStart == 0)
            return ip.ToFullString();
        string result = ip.Hextets
            .Skip(zerosAtStart)
            .Select(x => x.ToHexString(condensed))
            .Join(":");
        return "::" + result + ip.GetCidrSuffix();
    }

    /// <summary>
    /// Get the string representation of the IPv6 address shortened from the end.
    /// </summary>
    /// <param name="ip">The IPv6 address.</param>
    /// <param name="condensed">Should `0` values be condensed?</param>
    /// <returns>The string representation of the IPv6 address shortened from the end.</returns>
    public static string ShortenFromEnd(this IPv6 ip, bool condensed = true)
    {
        int zerosAtEnd = ip.Hextets
            .Reverse()
            .TakeWhile(x => x == 0)
            .Count();
        if (zerosAtEnd == 0)
            return ip.ToFullString();
        string result = ip.Hextets
            .Take(ip.Hextets.Length - zerosAtEnd)
            .Select(x => x.ToHexString(condensed))
            .Join(":");
        return result + "::" + ip.GetCidrSuffix();
    }


    /// <summary>
    /// Get the shortest string representation of the IPv6 address.
    /// </summary>
    /// <param name="ip">The IPv6 address.</param>
    /// <param name="condensed">Should `0` values be condensed?</param>
    /// <returns>The shortest string representation of the IPv6 address.</returns>
    public static string Shorten(this IPv6 ip, bool condensed = true)
    {
        int[] counts = ip.Hextets
            .Select((_, i) => ip
                .Hextets
                .Skip(i)
                .TakeWhile(xx => xx == 0)
                .Count())
            .ToArray();
        int zeroCount = counts.Max();
        if (zeroCount == 0)
            return ip.ToFullString();
        int startIndex = counts
            .Select((x, i) => x == zeroCount
                ? i
                : -1)
            .FirstOrDefault(x => x > -1);
        string before = ip.Hextets.Take(startIndex)
            .Select(x => x.ToHexString(condensed))
            .Join(":");
        string after = ip.Hextets.Skip(startIndex + zeroCount)
            .Select(x => x.ToHexString(condensed))
            .Join(":");
        return before + "::" + after + ip.GetCidrSuffix();
    }

    private static string GetCidrSuffix(this IPv6 ip)
    {
        return ip.Cidr is null
            ? string.Empty
            : "/" + ip.Cidr;
    }

    private static string ToHexString(this ushort hextet, bool condensed)
    {
        string format = condensed
            ? "x"
            : "x4";
        return hextet.ToString(format);
    }
}
