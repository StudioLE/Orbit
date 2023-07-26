using Microsoft.Extensions.Logging;
using StudioLE.Core.Serialization;

namespace Orbit.Core.Utils.Serialization.Yaml;


/// <summary>
/// Deserialize objects from YAML.
/// </summary>
public class YamlDeserializer : IDeserializer
{
    private readonly ILogger<YamlDeserializer> _logger;
    private readonly YamlDotNet.Serialization.IDeserializer _deserializer;

    /// <inheritdoc/>
    public string FileExtension => ".yaml";

    /// <summary>
    /// Create a new <see cref="YamlDeserializer"/>.
    /// </summary>
    /// <param name="logger">The Logger.</param>
    /// <param name="deserializer">The YAML deserializer to use.</param>
    public YamlDeserializer(ILogger<YamlDeserializer> logger, YamlDotNet.Serialization.IDeserializer deserializer)
    {
        _logger = logger;
        _deserializer = deserializer;
    }

    /// <summary>
    /// Create a new <see cref="YamlDeserializer"/>.
    /// </summary>
    /// <remarks>
    /// The default deserializer ignores unmatched properties.
    /// </remarks>
    public YamlDeserializer(ILogger<YamlDeserializer> logger)
    {
        _logger = logger;
        _deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNodeTypeResolver(new ReadOnlyCollectionNodeTypeResolver())
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc />
    public object? Deserialize(TextReader input, Type type)
    {
        try
        {
            return _deserializer.Deserialize(input, type);
        }
        catch (YamlDotNet.Core.YamlException e)
        {
            _logger.LogError(e, $"Failed to de-serialize {type}. {e.Message}");
            return null;
        }
    }
}
