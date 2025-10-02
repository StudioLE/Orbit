using StudioLE.Orbit.Clients;
using StudioLE.Orbit.Instances;
using StudioLE.Orbit.Schema;
using StudioLE.Extensions.System.Exceptions;
using StudioLE.Serialization;
using StudioLE.Storage.Files;

namespace StudioLE.Orbit.Configuration;

/// <summary>
/// Provides the shell commands to configure a <see cref="Server"/>.
/// </summary>
public class ServerConfigurationProvider
{
    internal const string FileName = "server-config.yml";
    private readonly IFileReader _reader;
    private readonly IFileWriter _writer;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="ServerConfigurationProvider"/>.
    /// </summary>
    public ServerConfigurationProvider(
        IFileReader reader,
        IFileWriter writer,
        ISerializer serializer,
        IDeserializer deserializer)
    {
        _reader = reader;
        _writer = writer;
        _serializer = serializer;
        _deserializer = deserializer;
    }

    /// <summary>
    /// Retrieve the server configuration.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <returns>The LXD configuration yaml, or <see langword="null"/> if it doesn't exist.</returns>
    public async Task<ServerConfiguration?> Get<T>(IEntityId<T> id) where T : struct, IEntity
    {
        await using Stream? stream = await _reader.Read(GetFilePath(id));
        if (stream is null)
            return null;
        using StreamReader reader = new(stream);
        object? obj = _deserializer.Deserialize(reader, typeof(IReadOnlyDictionary<string, ShellCommand[]>));
        if (obj is IReadOnlyDictionary<string, ShellCommand[]> dictionary)
            return new(dictionary);
        throw new();
    }


    /// <summary>
    /// Store the server configuration.
    /// </summary>
    /// <param name="id">The entity id.</param>
    /// <param name="config">The server configuration.</param>
    /// <returns><see langword="true"/> if the configuration is stored successfully, otherwise <see langword="false"/>.</returns>
    /// <exception cref="Exception">Thrown if the file provider is not physical.</exception>
    public async Task<bool> Put<T>(IEntityId<T> id, Dictionary<ServerId, ShellCommand[]> config) where T : struct, IEntity
    {
        string path = GetFilePath(id);
        await using Stream? stream = await _writer.OpenWrite(path, out string uri);
        if (stream is null)
            return false;
        await using StreamWriter writer = new(stream);
        _serializer.Serialize(writer, config);
        return !string.IsNullOrEmpty(uri);
    }

    private static string GetFilePath<T>(IEntityId<T> id) where T : struct, IEntity
    {
        return id switch
        {
            ClientId clientId => Path.Combine(ClientProvider.DirectoryName, clientId.ToString(), FileName),
            InstanceId instanceId => Path.Combine(InstanceProvider.DirectoryName, instanceId.ToString(), FileName),
            _ => throw new TypeSwitchException<IEntityId<T>>(id)
        };
    }
}
