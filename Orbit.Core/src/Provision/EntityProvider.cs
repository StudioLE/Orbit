using Microsoft.Extensions.FileProviders;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Serialization;
using StudioLE.Core.Serialization;

namespace Orbit.Core.Provision;

public class EntityProvider<T> : IEntityProvider<T> where T : class, IEntity
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
        if(file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if(!directory.Exists)
            directory.Create();
        if(file.Exists)
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

    public bool PutResource(IEntityId<T> id, string fileName, object obj, string? prefixYaml = null)
    {
        string path = Path.Combine(id.GetFilePath(), "..", fileName);
        IFileInfo file = _fileProvider.GetFileInfo(path);
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
        _serializer.Serialize(writer, obj);
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
