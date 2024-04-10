using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orbit.Utils.Serialization;
using StudioLE.Serialization.Yaml;
using YamlDotNet.Serialization;
using IDeserializer = StudioLE.Serialization.IDeserializer;
using ISerializer = StudioLE.Serialization.ISerializer;

namespace Orbit.Serialization;

public static class YamlHelpers
{
    public static ISerializer CreateSerializer(IServiceProvider services)
    {
        return CreateSerializer();
    }

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
            .Build();
        YamlDeserializer deserializer = new(logger, yamlDeserializer);
        return deserializer;
    }
}
