using System.Text.RegularExpressions;

namespace Orbit.Utils.Networking;

// ReSharper disable once InconsistentNaming
internal static class IPv4Parser
{
    private const string Octet = "[0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5]";
    private const string Cidr = "[0-9]|[1-2][0-9]|3[0-2]";
    private const string WithoutCidr = "^(" + Octet + @")\.(" + Octet + @")\.(" + Octet + @")\.(" + Octet + ")$";
    private const string WithCidr = "^(" + Octet + @")\.(" + Octet + @")\.(" + Octet + @")\.(" + Octet + ")/(" + Cidr + ")$";

    internal static IPv4? Parse(string source)
    {
        IPv4? withoutCidr = ParseWithoutCidr(source);
        if (withoutCidr is not null)
            return withoutCidr;
        IPv4? withCidr = ParseWithCidr(source);
        return withCidr;
    }

    private static IPv4? ParseWithoutCidr(string source)
    {
        Regex regex = new(WithoutCidr);
        Match match = regex.Match(source);
        if (!match.Success)
            return null;
        byte[] octets = match
            .Groups
            .Skip(1)
            .Take(4)
            .Select(g => byte.Parse(g.Value))
            .ToArray();
        return new(octets);
    }

    private static IPv4? ParseWithCidr(string source)
    {
        Regex regex = new(WithCidr);
        Match match = regex.Match(source);
        if (!match.Success)
            return null;
        byte[] octets = match
            .Groups
            .Skip(1)
            .Take(4)
            .Select(g => byte.Parse(g.Value))
            .ToArray();
        byte cidr = byte.Parse(match.Groups[5].Value);
        return new(octets, cidr);
    }
}
