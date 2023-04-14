namespace Orbit.Cli.Utils.CommandLine;

public static class KeyExtensions
{
    public static string ToLongOption(this string str)
    {
        return "--" + str.ToLower();
    }
}
