using System.Globalization;
using System.Text.RegularExpressions;

namespace Orbit.Utils.Networking;

// ReSharper disable once InconsistentNaming
internal static class MacAddressParser
{
    private const string Pattern = "^([a-f0-9]{2}):([a-f0-9]{2}):([a-f0-9]{2}):([a-f0-9]{2}):([a-f0-9]{2}):([a-f0-9]{2})$";

    public static MacAddress? Parse(string source)
    {
        Regex regex = new(Pattern, RegexOptions.IgnoreCase);
        Match match = regex.Match(source);
        if (!match.Success)
            return null;
        byte[] octets = match
            .Groups
            .Values
            .Skip(1)
            .Take(6)
            .Select(g => byte.Parse(g.Value, NumberStyles.HexNumber))
            .ToArray();
        return new(octets);
    }
}
