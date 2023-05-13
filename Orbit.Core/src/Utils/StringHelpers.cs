namespace Orbit.Core.Utils;

/// <summary>
/// Methods to help with <see cref="string"/>.
/// </summary>
public static class StringHelpers
{
    /// <inheritdoc cref="string.IsNullOrEmpty(string)"/>
    public static bool IsNullOrEmpty(this string @this)
    {
        return string.IsNullOrEmpty(@this);
    }

    /// <inheritdoc cref="string.IsNullOrWhiteSpace(string)"/>
    public static bool IsNullOrWhiteSpace(this string @this)
    {
        return string.IsNullOrWhiteSpace(@this);
    }

    /// <summary>
    /// Split a string into separate lines by either \r\n or \n.
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static IEnumerable<string> SplitIntoLines(this string @this)
    {
        return @this.Split("\r\n").SelectMany(x => x.Split("\n"));
    }
}
