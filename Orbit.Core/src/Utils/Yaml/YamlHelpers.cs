using System.Text;
using StudioLE.Extensions.System;

namespace Orbit.Utils.Yaml;

/// <summary>
/// Methods to help with YAML.
/// </summary>
public static class YamlHelpers
{
    /// <summary>
    /// The number of spaces to indent.
    /// </summary>
    private const int IndentSize = 2;
    private const string SingleQuoteRequired = ":,[]{}#&*!|>'%@`";
    private const string DoubleQuoteRequired = "\"\\";

    /// <summary>
    /// Serialize a collection as a YAML sequence.
    /// </summary>
    /// <param name="items">The items</param>
    /// <param name="indentLevel">The indentation level.</param>
    /// <returns>The collection serialized as a YAML sequence</returns>
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

    /// <summary>
    /// Serialize a string as a YAML string with quotes and escaping where necessary
    /// </summary>
    /// <remarks>
    /// Multi-line strings are not supported.
    /// </remarks>
    /// <param name="value">The value to serialize.</param>
    /// <returns>
    /// A YAML string with quotes and escaping where necessary.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown for multi-line strings.</exception>
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
