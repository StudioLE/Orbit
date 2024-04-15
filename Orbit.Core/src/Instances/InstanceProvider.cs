using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Serialization;

namespace Orbit.Instances;

/// <summary>
/// Retrieves and stores <see cref="Instance"/>s.
/// </summary>
public class InstanceProvider
{
    internal const string DirectoryName = "instances";
    private readonly IEntityFileProvider _fileProvider;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="EntityProvider{T}"/>.
    /// </summary>
    public InstanceProvider(IEntityFileProvider fileProvider, ISerializer serializer, IDeserializer deserializer)
    {
        _fileProvider = fileProvider;
        _serializer = serializer;
        _deserializer = deserializer;
    }

    /// <summary>
    /// Get the <see cref="Instance"/> with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <returns>
    /// The instance with the given <paramref name="id"/> if it exists; otherwise, <see langword="null"/>.
    /// </returns>
    public Instance? Get(InstanceId id)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(id));
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return _deserializer.Deserialize<Instance>(reader);
    }

    /// <summary>
    /// Store the <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns>
    /// <see langword="true"/> if the instance was stored; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Put(Instance instance)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(instance.Name));
        if (file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        if (file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        _serializer.Serialize(writer, instance);
        return true;
    }


    /// <summary>
    /// Get all stored instances.
    /// </summary>
    /// <returns>
    /// All the stored instances.
    /// </returns>
    public IEnumerable<Instance> GetAll()
    {
        return _fileProvider.GetDirectoryContents(DirectoryName)
            .Where(x => x.IsDirectory)
            .Select(x => Get(new(x.Name)))
            .OfType<Instance>();
    }

    private static string GetFilePath(InstanceId id)
    {
        return Path.Combine(DirectoryName, id.ToString(), id + ".yml");
    }
}
