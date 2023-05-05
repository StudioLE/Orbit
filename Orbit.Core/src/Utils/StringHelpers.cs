namespace Orbit.Core.Utils;

public static class StringHelpers
{
    public static bool IsNullOrEmpty(this string @this)
    {
        return string.IsNullOrEmpty(@this);
    }

    public static bool IsNullOrWhiteSpace(this string @this)
    {
        return string.IsNullOrWhiteSpace(@this);
    }

    public static IEnumerable<string> SplitIntoLines(this string @this)
    {
        return @this.Split("\r\n").SelectMany(x => x.Split("\n"));
    }
}
