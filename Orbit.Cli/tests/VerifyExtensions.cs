using Orbit.Core;
using StudioLE.Verify;
using YamlDotNet.Serialization;

namespace Orbit.Cli.Tests;

public static class VerifyExtensions
{
    public static Task AsYaml(this Verify verify, object obj)
    {
        ISerializer serializer = Yaml.Serializer();
        string yaml = serializer.Serialize(obj);
        return verify.String(yaml);
    }
}
