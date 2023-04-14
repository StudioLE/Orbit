using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Orbit.Core.Schema;

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

}
