using StudioLE.CommandLine.Utils.Logging.TestLogger;
using StudioLE.Core.System;
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

    public static Task AsString(this Verify.Verify verify, TestLogger logger)
    {
        string value = logger
            .Logs
            .Select(x => x.Message)
            .Join();
        return verify.String(value);
    }
}
