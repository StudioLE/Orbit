using StudioLE.Orbit.Schema;
using StudioLE.Serialization;
using StudioLE.Storage.Files;

namespace StudioLE.Orbit.Servers;

/// <summary>
/// Provide and store <see cref="Server"/>.
/// </summary>
public class ServerProvider
{
    internal const string DirectoryName = "servers";
    private readonly IFileReader _reader;
    private readonly IFileWriter _writer;
    private readonly IDirectoryReader _index;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="ServerProvider"/>.
    /// </summary>
    public ServerProvider(
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
    /// Get the <see cref="Server"/> with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The server id.</param>
    /// <returns>
    /// The server with the given <paramref name="id"/> if it exists; otherwise, <see langword="null"/>.
    /// </returns>
    public async Task<Server?> Get(ServerId id)
    {
        string path = GetFilePath(id);
        await using Stream? stream = await _reader.Read(path);
        if (stream is null)
            return null;
        using StreamReader reader = new(stream);
        string yaml = await reader.ReadToEndAsync();
        return _deserializer.Deserialize<Server>(yaml);
    }

    /// <summary>
    /// Store the <paramref name="server"/>.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// <see langword="true"/> if the server was stored; otherwise, <see langword="false"/>.
    /// </returns>
    public async Task<bool> Put(Server server)
    {
        string path = GetFilePath(server.Name);
        await using Stream? stream = await _writer.OpenWrite(path, out string uri);
        if (stream is null)
            return false;
        await using StreamWriter writer = new(stream);
        _serializer.Serialize(writer, server);
        return !string.IsNullOrEmpty(uri);
    }

    /// <summary>
    /// Get all stored servers.
    /// </summary>
    /// <returns>
    /// All the stored servers.
    /// </returns>
    public async Task<Server[]> GetAll()
    {
        IEnumerable<string>? names = await _index.GetDirectoryNames(DirectoryName);
        if (names is null)
            return Array.Empty<Server>();
        IEnumerable<Task<Server?>> serversAsync = names
            .Select(x => Get(new(x)));
        Server?[] serversTask = await Task.WhenAll(serversAsync);
        Server[] servers = serversTask
            .OfType<Server>()
            .ToArray();
        return servers;
    }

    private static string GetFilePath(ServerId id)
    {
        return Path.Combine(DirectoryName, id.ToString(), id + ".yml");
    }
}
