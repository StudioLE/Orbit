using Orbit.Core.Schema;
using YamlDotNet.Serialization;

namespace Orbit.Core;

public static class InstanceApi
{
    // TODO: Abstract this to a FileProvider
    private const string OrbitDirectory = @"E:\Repos\Infrastructure\Orbit\.orbit";

    public static IEnumerable<string> GetIds()
    {
        return Directory.EnumerateDirectories(OrbitDirectory)
            .Select(Path.GetFileName);
    }

    public static Instance? Get(string instanceId)
    {
        FileInfo file = new(Path.Combine(OrbitDirectory, instanceId, instanceId + ".yml"));
        if (!file.Exists)
            return null;
        using StreamReader reader = new(file.FullName);
        IDeserializer deserializer = Yaml.Deserializer();
        return deserializer.Deserialize<Instance>(reader);
    }

    public static bool Put(Instance instance)
    {
        return PutResource(instance.Id, instance.Id + ".yml", instance);
    }

    public static bool PutResource(string instanceId, string fileName, object obj)
    {
        FileInfo file = new(Path.Combine(OrbitDirectory, instanceId, fileName));
        DirectoryInfo directory = file.Directory ?? throw new("Directory was unexpectedly null.");
        if(!directory.Exists)
            directory.Create();
        if(file.Exists)
            return false;
        using StreamWriter writer = file.CreateText();
        writer.WriteLine("#cloud-config");
        writer.WriteLine();
        ISerializer serializer = Yaml.Serializer();
        serializer.Serialize(writer, obj);
        return true;
    }
}
