using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Serialization;

namespace Orbit.Clients;

/// <summary>
/// Retrieves and stores <see cref="Client"/>s.
/// </summary>
public class ClientProvider
{
    internal const string DirectoryName = "clients";
    private readonly IEntityFileProvider _fileProvider;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="ClientProvider"/>.
    /// </summary>
    public ClientProvider(IEntityFileProvider fileProvider, ISerializer serializer, IDeserializer deserializer)
    {
        _fileProvider = fileProvider;
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
    public Client? Get(ClientId id)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(id));
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
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
    public bool Put(Client client)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(client.Name));
        if (file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        if (file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        _serializer.Serialize(writer, client);
        return true;
    }


    /// <summary>
    /// Get all stored clients.
    /// </summary>
    /// <returns>
    /// All the stored clients.
    /// </returns>
    public IEnumerable<Client> GetAll()
    {
        return _fileProvider.GetDirectoryContents(DirectoryName)
            .Where(x => x.IsDirectory)
            .Select(x => Get(new(x.Name)))
            .OfType<Client>();
    }

    private static string GetFilePath(ClientId id)
    {
        return Path.Combine(DirectoryName, id.ToString(), id + ".yml");
    }
}
