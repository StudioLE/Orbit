namespace Orbit.Core.Utils.Serialization.Yaml;


/// <summary>
/// Deserialize objects from YAML.
/// </summary>
public class YamlDeserializer : IDeserializer
{
    private readonly YamlDotNet.Serialization.IDeserializer _deserializer;

    /// <inheritdoc/>
    public string FileExtension => ".yaml";

    /// <summary>
    /// Create a new <see cref="YamlDeserializer"/>.
    /// </summary>
    /// <param name="deserializer">The YAML deserializer to use.</param>
    public YamlDeserializer(YamlDotNet.Serialization.IDeserializer deserializer)
    {
        _deserializer = deserializer;
    }

    /// <summary>
    /// Create a new <see cref="YamlDeserializer"/>.
    /// </summary>
    /// <remarks>
    /// The default deserializer ignores unmatched properties.
    /// </remarks>
    public YamlDeserializer()
    {
        _deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNodeTypeResolver(new ReadOnlyCollectionNodeTypeResolver())
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc />
    public T Deserialize<T>(string input)
    {
        return _deserializer.Deserialize<T>(input);
    }

    /// <inheritdoc />
    public T Deserialize<T>(TextReader input)
    {
        return _deserializer.Deserialize<T>(input);
    }

    /// <inheritdoc />
    public object? Deserialize(string input, Type type)
    {
        return _deserializer.Deserialize(input, type);
    }

    /// <inheritdoc />
    public object? Deserialize(TextReader input, Type type)
    {
        return _deserializer.Deserialize(input, type);
    }
}
