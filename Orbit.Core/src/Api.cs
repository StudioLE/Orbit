using Orbit.Core.Schema;
using StudioLE.Core.Results;
using YamlDotNet.Serialization;

namespace Orbit.Core;

public static class Api
{
    private const string OrbitDirectory = @"E:\Repos\Infrastructure\Orbit\.orbit";

    public static IEnumerable<string> GetInstanceIds()
    {
        return Directory.EnumerateDirectories(OrbitDirectory)
            .Select(Path.GetFileName);
    }

    public static IResult<Instance> TryGetInstance(string instanceId)
    {
        FileInfo file = GetInstanceFile(instanceId);
        if (!file.Exists)
            return new Failure<Instance>("The file does not exist.");
        using StreamReader reader = new(file.FullName);
        IDeserializer deserializer = Yaml.Deserializer();
        Instance instance = deserializer.Deserialize<Instance>(reader);
        return new Success<Instance>(instance);
    }

    public static IResult TryWriteInstance(Instance instance, bool allowOverwrite)
    {
        FileInfo file = GetInstanceFile(instance.Id);
        if (!allowOverwrite && file.Exists)
            return new Failure("The file already exists.");
        if (!file.Directory!.Exists)
            file.Directory.Create();
        using StreamWriter writer = new(file.FullName);
        ISerializer serializer = Yaml.Serializer();
        serializer.Serialize(writer, instance);
        return new Success();
    }

    private static FileInfo GetInstanceFile(string instanceId)
    {
        return new(Path.Combine(OrbitDirectory, instanceId, instanceId + ".yml"));
    }
}
