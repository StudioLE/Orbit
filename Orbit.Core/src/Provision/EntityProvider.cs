using Microsoft.Extensions.FileProviders;
using Orbit.Schema;
using StudioLE.Serialization;

namespace Orbit.Provision;

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
        IFileInfo file = _fileProvider.GetFileInfo(id.GetFilePath());
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return _deserializer.Deserialize<T>(reader);
    }

    /// <inheritdoc />
    public bool Put(IEntityId<T> id, T value)
    {
        IFileInfo file = _fileProvider.GetFileInfo(id.GetFilePath());
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
        string directory = EntityFileProvider.GetDirectory<T>();
        string fileExtension = _deserializer.FileExtension;
        // TODO: Get file extension from deserializer.
        return _fileProvider.GetDirectoryContents(directory)
            .Where(x => x.IsDirectory)
            .Select(x => x.Name);
    }

    public bool PutResource(IEntityId<T> id, string fileName, string content)
    {
        string path = Path.Combine(id.GetFilePath(), "..", fileName);
        IFileInfo file = _fileProvider.GetFileInfo(path);
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        using StreamWriter writer = physicalFile.CreateText();
        writer.Write(content);
        return true;
    }

    public string? GetResource(IEntityId<T> id, string fileName)
    {
        string path = Path.Combine(id.GetFilePath(), "..", fileName);
        IFileInfo file = _fileProvider.GetFileInfo(path);
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
