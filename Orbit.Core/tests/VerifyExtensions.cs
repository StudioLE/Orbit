using Orbit.Core.Schema;
using StudioLE.Verify;
using YamlDotNet.Serialization;

namespace Orbit.Core.Tests;

public static class VerifyExtensions
{
    public static Task AsYaml(this Verify verify, Instance instance)
    {
        ISerializer serializer = Yaml.Serializer();
        string yaml = serializer.Serialize(instance);
        return verify.String(yaml);
    }
}
