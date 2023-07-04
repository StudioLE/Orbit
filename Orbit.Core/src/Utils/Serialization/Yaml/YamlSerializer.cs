using StudioLE.Core.Serialization;

namespace Orbit.Core.Utils.Serialization.Yaml;

/// <summary>
/// Serialize objects to YAML.
/// </summary>
public class YamlSerializer : ISerializer
{
    private readonly YamlDotNet.Serialization.ISerializer _serializer;

    /// <inheritdoc />
    public string FileExtension => ".yaml";

    /// <summary>
    /// Create a new <see cref="YamlSerializer"/>.
    /// </summary>
    /// <param name="serializer">The YAML serializer to use.</param>
    public YamlSerializer(YamlDotNet.Serialization.ISerializer serializer)
    {
        _serializer = serializer;
    }

    /// <summary>
    /// Create a new <see cref="YamlSerializer"/>.
    /// </summary>
    /// <remarks>
    /// The default serializer does not use aliases.
    /// </remarks>
    public YamlSerializer()
    {
        _serializer = new YamlDotNet.Serialization.SerializerBuilder()
            .DisableAliases()
            .Build();
    }

    /// <inheritdoc />
    public void Serialize(TextWriter writer, object? obj)
    {
        _serializer.Serialize(writer, obj);
    }
}
