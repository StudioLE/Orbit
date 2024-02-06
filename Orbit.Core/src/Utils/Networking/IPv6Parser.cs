using System.Text.RegularExpressions;

namespace Orbit.Utils.Networking;

// ReSharper disable once InconsistentNaming
internal static class IPv6Parser
{
    private const string Hextet = "[0-9a-f]{1,4}";
    private const string Cidr = "[0-9]|[1-9][0-9]|1[0-1][0-9]|12[0-8]";
    private const string Full = "^(?:(" + Hextet + "):){7}(" + Hextet + ")(?:/(" + Cidr + "))?$";
    private const string Shortened =
        "^(?:(" + Hextet + "):){0,6}(" + Hextet + ")?::(?:(" + Hextet + "):){0,6}(" + Hextet + ")?(?:/(" + Cidr + "))?$";

    internal static IPv6? Parse(string source)
    {
        IPv6? full = ParseFull(source);
        if (full is not null)
            return full;
        IPv6? shortened = ParseShortened(source);
        return shortened;
    }

    private static IPv6? ParseFull(string source)
    {
        Regex regex = new(Full);
        Match match = regex.Match(source);
        if (!match.Success)
            return null;
        ushort[] hextets = Array.Empty<string>()
            .Concat(match.Groups[1].Captures.Select(x => x.Value))
            .Append(match.Groups[2].Captures[0].Value)
            .Select(ToHextet)
            .ToArray();
        string? cidrString = match.Groups[3].Captures.FirstOrDefault()?.Value;
        byte? cidr = cidrString is null
            ? null
            : byte.Parse(cidrString);
        return new IPv6(hextets, cidr);
    }

    private static IPv6? ParseShortened(string source)
    {
        Regex regex = new(Shortened);
        Match match = regex.Match(source);
        if (!match.Success)
            return null;
        ushort[] start = Array.Empty<string>()
            .Concat(match.Groups[1].Captures.Select(x => x.Value))
            .Append(match.Groups[2].Captures.FirstOrDefault()?.Value)
            .OfType<string>()
            .Select(ToHextet)
            .ToArray();
        ushort[] end = Array.Empty<string>()
            .Concat(match.Groups[3].Captures.Select(x => x.Value))
            .Append(match.Groups[4].Captures.FirstOrDefault()?.Value)
            .OfType<string>()
            .Select(ToHextet)
            .ToArray();
        int count = start.Length + end.Length;
        if (count > 8)
            return null;
        string? cidrString = match.Groups[5].Captures.FirstOrDefault()?.Value;
        byte? cidr = cidrString is null
            ? null
            : byte.Parse(cidrString);
        return new IPv6(start, end, cidr);
    }

    private static ushort ToHextet(string source)
    {
        return (ushort)Convert.ToInt32(source, 16);
    }
}
