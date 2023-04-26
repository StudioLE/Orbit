using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StudioLE.CommandLine.Tests;

public static class VerifyExtensions
{
    public static Task AsYaml(this Verify.Verify verify, object obj)
    {
        ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        string yaml = serializer.Serialize(obj);
        return verify.String(yaml);
    }
}
