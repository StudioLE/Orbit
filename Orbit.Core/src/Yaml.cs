using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orbit.Core;

public static class Yaml
{
    private static readonly INamingConvention _namingConvention = UnderscoredNamingConvention.Instance;

    public static ISerializer Serializer()
    {
        return new SerializerBuilder()
            .WithNamingConvention(_namingConvention)
            .Build();
    }

    public static IDeserializer Deserializer()
    {
        return new DeserializerBuilder()
            .WithNamingConvention(_namingConvention)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public static string Serialize(object obj)
    {
        ISerializer serializer = Serializer();
        return serializer.Serialize(obj);
    }

    public static void SetValue(this YamlNode @this, string value, ScalarStyle? style = null)
    {
        if (@this is not YamlScalarNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Value = value;
        if (style is not null)
            node.Style = (ScalarStyle)style;
    }

    public static void AddRange(this YamlNode @this, IEnumerable<string> values)
    {
        if (@this is not YamlSequenceNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        foreach (string value in values)
            node.Add(value);
    }

    public static void Add(this YamlNode @this, string value)
    {
        if (@this is not YamlSequenceNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Add(value);
    }

    public static void Add(this YamlNode @this, YamlNode value)
    {
        if (@this is not YamlSequenceNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Add(value);
    }

    public static void Replace(this YamlNode @this, string key, YamlNode replacement)
    {
        if (@this is not YamlMappingNode node)
            throw new InvalidCastException($"Failed to set value of {@this.GetType()}. Expected a {nameof(YamlScalarNode)}");
        node.Children[key] = replacement;
    }
}
