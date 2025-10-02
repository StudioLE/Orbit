using StudioLE.Orbit.Schema;
using StudioLE.Serialization;
using StudioLE.Storage.Files;

namespace StudioLE.Orbit.Clients;

/// <summary>
/// Provide and store <see cref="Client"/>.
/// </summary>
public class ClientProvider
{
    internal const string DirectoryName = "clients";
    private readonly IFileReader _reader;
    private readonly IFileWriter _writer;
    private readonly IDirectoryReader _index;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="ClientProvider"/>.
    /// </summary>
    public ClientProvider(
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
    /// Get the <see cref="Client"/> with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The client id.</param>
    /// <returns>
    /// The client with the given <paramref name="id"/> if it exists; otherwise, <see langword="null"/>.
    /// </returns>
    public async Task<Client?> Get(ClientId id)
    {
        string path = GetFilePath(id);
        await using Stream? stream = await _reader.Read(path);
        if (stream is null)
            return null;
        using StreamReader reader = new(stream);
        return _deserializer.Deserialize<Client>(reader);
    }

    /// <summary>
    /// Store the <paramref name="client"/>.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <returns>
    /// <see langword="true"/> if the client was stored; otherwise, <see langword="false"/>.
    /// </returns>
    public async Task<bool> Put(Client client)
    {
        string path = GetFilePath(client.Name);
        await using Stream? stream = await _writer.OpenWrite(path, out string uri);
        if (stream is null)
            return false;
        await using StreamWriter writer = new(stream);
        _serializer.Serialize(writer, client);
        return !string.IsNullOrEmpty(uri);
    }

    /// <summary>
    /// Get all stored clients.
    /// </summary>
    /// <returns>
    /// All the stored clients.
    /// </returns>
    public async Task<IAsyncEnumerable<Client>> GetAll()
    {
        IEnumerable<string>? names = await _index.GetDirectoryNames(DirectoryName);
        if (names is null)
            return AsyncEnumerable.Empty<Client>();
        IAsyncEnumerable<Client> clients = names
            .ToAsyncEnumerable()
            .SelectAwait(async x => await Get(new(x)))
            .Where(x => x is Client)
            .Select(x => (Client)x!)
            .Select(x => x);
        return clients;
    }

    private static string GetFilePath(ClientId id)
    {
        return Path.Combine(DirectoryName, id.ToString(), id + ".yml");
    }
}
