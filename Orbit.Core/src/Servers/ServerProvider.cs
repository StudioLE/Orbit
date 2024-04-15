using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Serialization;

namespace Orbit.Servers;

/// <summary>
/// Retrieves and stores <see cref="Server"/>s.
/// </summary>
public class ServerProvider
{
    internal const string DirectoryName = "servers";
    private readonly IEntityFileProvider _fileProvider;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="ServerProvider"/>.
    /// </summary>
    public ServerProvider(IEntityFileProvider fileProvider, ISerializer serializer, IDeserializer deserializer)
    {
        _fileProvider = fileProvider;
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
    public Server? Get(ServerId id)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(id));
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return _deserializer.Deserialize<Server>(reader);
    }

    /// <summary>
    /// Store the <paramref name="server"/>.
    /// </summary>
    /// <param name="server">The server.</param>
    /// <returns>
    /// <see langword="true"/> if the server was stored; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Put(Server server)
    {
        IFileInfo file = _fileProvider.GetFileInfo(GetFilePath(server.Name));
        if (file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        if (file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        _serializer.Serialize(writer, server);
        return true;
    }


    /// <summary>
    /// Get all stored servers.
    /// </summary>
    /// <returns>
    /// All the stored servers.
    /// </returns>
    public IEnumerable<Server> GetAll()
    {
        return _fileProvider.GetDirectoryContents(DirectoryName)
            .Where(x => x.IsDirectory)
            .Select(x => Get(new(x.Name)))
            .OfType<Server>();
    }

    private static string GetFilePath(ServerId id)
    {
        return Path.Combine(DirectoryName, id.ToString(), id + ".yml");
    }
}

