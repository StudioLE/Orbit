using Microsoft.Extensions.FileProviders;
using YamlDotNet.Serialization;

namespace Orbit.Core.Providers;

public static class ProviderHelpers
{
    public static T? ReadYamlFileAs<T>(this IFileInfo file) where T : class
    {
        if (!file.Exists)
            return null;
        Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        IDeserializer deserializer = Yaml.Deserializer();
        return deserializer.Deserialize<T>(reader);
    }

    public static string? ReadTextFile(this IFileInfo file)
    {
        if (!file.Exists)
            return null;
        Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    public static bool WriteYamlFile(this IFileInfo file, object obj, string? prefixYaml = null)
    {
        if(file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if(!directory.Exists)
            directory.Create();
        if(file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        if (!string.IsNullOrEmpty(prefixYaml))
            writer.WriteLine(prefixYaml);
        ISerializer serializer = Yaml.Serializer();
        serializer.Serialize(writer, obj);
        return true;
    }

    public static bool WriteTextFile(this IFileInfo file, string content)
    {
        if(file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if(!directory.Exists)
            directory.Create();
        if(file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        writer.Write(content);
        return true;
    }
}
