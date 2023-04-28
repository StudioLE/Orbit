using System.Text.RegularExpressions;

namespace StudioLE.CommandLine.Utils;

public static class StringHelpers
{
    public static string PascalToTitleCase(string str)
    {
        string res1 = Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}");
        string res2 = Regex.Replace(res1, "([A-Z]+)([A-Z][a-z])", m => $"{m.Groups[1]} {m.Groups[2]}");
        return res2;
    }
}
