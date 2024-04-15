using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orbit.Utils.Networking;
using Orbit.Utils.Serialization;
using StudioLE.Serialization.Yaml;
using YamlDotNet.Serialization;
using IDeserializer = StudioLE.Serialization.IDeserializer;
using ISerializer = StudioLE.Serialization.ISerializer;

namespace Orbit.Serialization;

/// <summary>
/// Methods to help with YAML serialization.
/// </summary>
public static class YamlHelpers
{
    /// <summary>
    /// Create an <see cref="ISerializer"/>.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <returns>An <see cref="ISerializer"/>.</returns>
    public static ISerializer CreateSerializer(IServiceProvider services)
    {
        return CreateSerializer();
    }

    /// <summary>
    /// Create an <see cref="IDeserializer"/>.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <returns>An <see cref="IDeserializer"/>.</returns>
    public static IDeserializer CreateDeserializer(IServiceProvider services)
    {
        ILogger<YamlDeserializer> logger = services.GetRequiredService<ILogger<YamlDeserializer>>();
        return CreateDeserializer(logger);
    }

    private static ISerializer CreateSerializer()
    {
        ParseableYamlConverter converter = new();
        YamlDotNet.Serialization.ISerializer yamlSerializer = new SerializerBuilder()
            .DisableAliases()
            .WithLiteralMultilineStrings()
            .WithTypeConverter(converter)
            .WithTypeConverter(new MacAddressYamlConverter())
            .Build();
        YamlSerializer serializer = new(yamlSerializer);
        return serializer;
    }

    private static IDeserializer CreateDeserializer(ILogger<YamlDeserializer> logger)
    {
        ParseableYamlConverter converter = new();
        YamlDotNet.Serialization.IDeserializer yamlDeserializer = new DeserializerBuilder()
            .WithNodeTypeResolver(new ReadOnlyCollectionNodeTypeResolver())
            .IgnoreUnmatchedProperties()
            .WithTypeConverter(converter)
            .WithTypeConverter(new MacAddressYamlConverter())
            .Build();
        YamlDeserializer deserializer = new(logger, yamlDeserializer);
        return deserializer;
    }
}
