using Orbit.Schema;
using StudioLE.Serialization;
using StudioLE.Storage.Files;

namespace Orbit.Instances;

/// <summary>
/// Provide and store <see cref="Instance"/>.
/// </summary>
public class InstanceProvider
{
    internal const string DirectoryName = "instances";
    private readonly IFileReader _reader;
    private readonly IFileWriter _writer;
    private readonly IDirectoryReader _index;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="InstanceProvider"/>.
    /// </summary>
    public InstanceProvider(
        IFileReader reader,
        IFileWriter writer,
        IDirectoryReader index,
        ISerializer serializer,
        IDeserializer deserializer)
    {
        _reader = reader;
        _writer = writer;
        _index = index;
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
    public async Task<Instance?> Get(InstanceId id)
    {
        string path = GetFilePath(id);
        await using Stream? stream = await _reader.Read(path);
        if (stream is null)
            return null;
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
    public async Task<bool> Put(Instance instance)
    {
        string path = GetFilePath(instance.Name);
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        _serializer.Serialize(writer, instance);
        await writer.FlushAsync();
        stream.Seek(0, SeekOrigin.Begin);
        string? uri = await _writer.Write(path, stream);
        return uri is not null;
    }

    /// <summary>
    /// Get all stored instances.
    /// </summary>
    /// <returns>
    /// All the stored instances.
    /// </returns>
    public async Task<IAsyncEnumerable<Instance>> GetAll()
    {
        IEnumerable<string>? names = await _index.GetDirectoryNames(DirectoryName);
        if (names is null)
            return AsyncEnumerable.Empty<Instance>();
        IAsyncEnumerable<Instance> instances = names
            .ToAsyncEnumerable()
            .SelectAwait(async x => await Get(new(x)))
            .Where(x => x is Instance)
            .Select(x => (Instance)x!)
            .Select(x => x);
        return instances;
    }

    private static string GetFilePath(InstanceId id)
    {
        return Path.Combine(DirectoryName, id.ToString(), id + ".yml");
    }
}
