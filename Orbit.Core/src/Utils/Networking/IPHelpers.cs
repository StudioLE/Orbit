using System.Text.RegularExpressions;

namespace Orbit.Utils.Networking;

/// <summary>
/// Methods to help with <see cref="IPv4"/> and <see cref="IPv6"/>.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class IPHelpers
{
    // ReSharper disable once InconsistentNaming
    internal const string IPv4Regex = @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

    /// <summary>
    /// Is <paramref name="str"/> a valid IPv4 address?
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns><see langword="true"/> if <paramref name="str"/> is a valid IPv4 address, <see langword="false"/> otherwise.</returns>
    public static bool IsIPv4(string str)
    {
        Regex regex = new(IPv4Regex);
        return regex.IsMatch(str);
    }

    /// <summary>
    /// Is <paramref name="str"/> a valid IPv6 address?
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns><see langword="true"/> if <paramref name="str"/> is a valid IPv6 address, <see langword="false"/> otherwise.</returns>
    public static bool IsIPv6(string str)
    {
        IPv6? ipv6 = IPv6Parser.Parse(str);
        return ipv6 is not null;
    }

    /// <summary>
    /// Remove the CIDR from an IP address.
    /// </summary>
    public static string RemoveCidr(string ip)
    {
        return ip.Split('/')[0];
    }
}
