using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orbit.Core;

/// <summary>
/// Methods to help with YAML.
/// </summary>
public static class Yaml
{
    private static readonly INamingConvention _namingConvention = UnderscoredNamingConvention.Instance;

    /// <summary>
    /// The default Yaml serializer.
    /// </summary>
    public static ISerializer Serializer()
    {
        return new SerializerBuilder()
            .WithNamingConvention(_namingConvention)
            .Build();
    }

    /// <summary>
    /// The default Yaml de-serializer.
    /// </summary>
    public static IDeserializer Deserializer()
    {
        return new DeserializerBuilder()
            .WithNamingConvention(_namingConvention)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc cref="ISerializer.Serialize(object)"/>
    public static string Serialize(object obj)
    {
        ISerializer serializer = Serializer();
        return serializer.Serialize(obj);
    }


    /// <inheritdoc cref="IDeserializer.Deserialize{T}(string)"/>
    public static T Deserialize<T>(string yaml)
    {
        IDeserializer deserializer = Deserializer();
        return deserializer.Deserialize<T>(yaml);
    }

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
