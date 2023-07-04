using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace Orbit.Core;

/// <summary>
/// Methods to help with YAML.
/// </summary>
public static class Yaml
{
    /// <summary>
    /// Set the value of a <see cref="YamlScalarNode"/>.
    /// </summary>
    public static void SetValue(this YamlNode @this, string value, ScalarStyle? style = null)
    {
        if (@this is not YamlScalarNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Value = value;
        if (style is not null)
            node.Style = (ScalarStyle)style;
    }

    /// <summary>
    /// Set the sequence style of a <see cref="YamlSequenceNode"/>.
    /// </summary>
    public static void SetSequenceStyle(this YamlNode @this, SequenceStyle style)
    {
        if (@this is not YamlSequenceNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(SetSequenceStyle)}");
        node.Style = style;
    }

    /// <summary>
    /// Add a range of values to a <see cref="YamlSequenceNode"/>.
    /// </summary>
    public static void AddRange(this YamlNode @this, IEnumerable<string> values)
    {
        if (@this is not YamlSequenceNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        foreach (string value in values)
            node.Add(value);
    }

    /// <summary>
    /// Add a value to a <see cref="YamlSequenceNode"/>.
    /// </summary>
    public static void Add(this YamlNode @this, string value)
    {
        if (@this is not YamlSequenceNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Add(value);
    }

    /// <summary>
    /// Add a value to a <see cref="YamlSequenceNode"/>.
    /// </summary>
    public static void Add(this YamlNode @this, YamlNode value)
    {
        if (@this is not YamlSequenceNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Add(value);
    }

    /// <summary>
    /// Replace a <see cref="YamlNode"/>.
    /// </summary>
    public static void Replace(this YamlNode @this, string key, YamlNode replacement)
    {
        if (@this is not YamlMappingNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Children[key] = replacement;
    }
}
