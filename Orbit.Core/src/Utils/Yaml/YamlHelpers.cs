using System.Text;
using StudioLE.Extensions.System;

namespace Orbit.Utils.Yaml;

public static class YamlHelpers
{
    public const int IndentSize = 2;
    private const string SingleQuoteRequired = ":,[]{}#&*!|>'%@`";
    private const string DoubleQuoteRequired = "\"\\";

    public static string AsYamlSequence(this IReadOnlyCollection<string> items, int indentLevel = 0)
    {
        if (items.Count == 0)
            return " []";
        StringBuilder output = new();
        string indent = string.Concat(Enumerable.Repeat(" ", indentLevel * IndentSize));
        foreach (string item in items)
        {
            output.AppendLine();
            output.Append($"{indent}- {item.AsYamlString()}");
        }
        return output.ToString();
    }

    public static string AsYamlString(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "''";
        if (value.Contains("\n") || value.Contains("\r"))
            throw new NotSupportedException("Multi-line strings are not supported.");
        value = EscapeStart(value);
        value = EscapeEnd(value);
        return value;
    }

    private static string EscapeStart(this string value)
    {
        char firstChar = value.First();
        if (SingleQuoteRequired.Contains(firstChar))
            return value
                .Escape('\'')
                .Wrap('\'');
        if (DoubleQuoteRequired.Contains(firstChar))
            return value
                .Escape('"')
                .Wrap('"');
        return value;
    }

    private static string EscapeEnd(string value)
    {
        if (value.EndsWith("::"))
            return value
                .Escape('\'')
                .Wrap('\'');
        return value;
    }

    internal static string Indent(this string value, int indentLevel, bool indentEmptyLines = false)
    {
        string indentation = string.Concat(Enumerable.Repeat(" ", indentLevel * IndentSize));
        return value
            .Split("\n")
            .Select(x => !indentEmptyLines && string.IsNullOrEmpty(x) ? string.Empty : indentation + x)
            .Join("\n");
    }

    private static string Escape(this string value, char unescapedCharacter)
    {
        return value.Replace(unescapedCharacter.ToString(), @"\" + unescapedCharacter);
    }

    private static string Wrap(this string str, char quote)
    {
        return $"{quote}{str}{quote}";
    }
}
