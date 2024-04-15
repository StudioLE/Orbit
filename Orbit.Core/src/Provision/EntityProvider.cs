using Microsoft.Extensions.FileProviders;
using Orbit.Schema;
using StudioLE.Extensions.System.Exceptions;
using StudioLE.Serialization;

namespace Orbit.Provision;

/// <summary>
/// A provider for entities that stores them as files.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EntityProvider<T> : IEntityProvider<T> where T : struct, IEntity
{
    private readonly IEntityFileProvider _fileProvider;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="EntityProvider{T}"/>.
    /// </summary>
    public EntityProvider(IEntityFileProvider fileProvider, ISerializer serializer, IDeserializer deserializer)
    {
        _fileProvider = fileProvider;
        _serializer = serializer;
        _deserializer = deserializer;
    }

    /// <inheritdoc/>
    public T? Get(IEntityId<T> id)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(id));
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return _deserializer.Deserialize<T>(reader);
    }

    /// <inheritdoc />
    public bool Put(IEntityId<T> id, T value)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(id));
        if (file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        if (file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        _serializer.Serialize(writer, value);
        return true;
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetIndex()
    {
        string directory = GetSubDirectory();
        return _fileProvider.GetDirectoryContents(directory)
            .Where(x => x.IsDirectory)
            .Select(x => x.Name);
    }

    /// <inheritdoc/>
    public bool PutArtifact(IEntityId<T> id, string fileName, string content)
    {
        string path = Path.Combine(GetFilePath(id), "..", "artifacts", fileName);
        IFileInfo file = _fileProvider.GetFileInfo(path);
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        using StreamWriter writer = physicalFile.CreateText();
        writer.Write(content);
        return true;
    }

    /// <inheritdoc/>
    public string? GetArtifact(IEntityId<T> id, string fileName)
    {

        string path = Path.Combine(GetFilePath(id), "..", "artifacts", fileName);
        IFileInfo file = _fileProvider.GetFileInfo(path);
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static string GetSubDirectory()
    {
        Type type = typeof(T);
        if (type == typeof(Instance))
            return "instances";
        if (type == typeof(Server))
            return "servers";
        if (type == typeof(Client))
            return "clients";
        throw new("Invalid entity type.");
    }

    private static string GetSubDirectory(IEntityId<T> id)
    {
        return id switch
        {
            ClientId _ => "clients",
            InstanceId _ => "instances",
            ServerId _ => "servers",
            _ => throw new TypeSwitchException<IEntityId<T>>(id)
        };
    }

    private static string GetFilePath(IEntityId<T> id)
    {
        string subDirectory = GetSubDirectory(id);
        string name = id.ToString() ?? throw new("Id was unexpectedly null.");
        return Path.Combine(subDirectory, name, name + ".yml");
    }



}
