using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Extensions.System.Exceptions;
using StudioLE.Serialization;

namespace Orbit.Configuration;

/// <summary>
/// Provides the shell commands to configure a <see cref="Server"/>.
/// </summary>
public class ServerConfigurationProvider
{
    private readonly IEntityFileProvider _fileProvider;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    /// <summary>
    /// DI constructor for <see cref="ServerConfigurationProvider"/>.
    /// </summary>
    public ServerConfigurationProvider(
        IEntityFileProvider fileProvider,
        ISerializer serializer,
        IDeserializer deserializer)
    {
        _fileProvider = fileProvider;
        _serializer = serializer;
        _deserializer = deserializer;
    }

    /// <summary>
    /// Retrieve the server configuration.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <returns>The LXD configuration yaml, or <see langword="null"/> if it doesn't exist.</returns>
    public ServerConfiguration? Get<T>(IEntityId<T> id) where T : struct, IEntity
    {
        IFileInfo file = GetFileInfo(id);
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        object? obj = _deserializer.Deserialize(reader, typeof(IReadOnlyDictionary<string, ShellCommand[]>));
        if(obj is IReadOnlyDictionary<string, ShellCommand[]> dictionary)
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
    public bool Put<T>(IEntityId<T> id, Dictionary<ServerId, ShellCommand[]> config) where T : struct, IEntity
    {
        IFileInfo file = GetFileInfo(id);
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        using StreamWriter writer = physicalFile.CreateText();
        string yaml = _serializer.Serialize(config);
        writer.Write(yaml);
        return true;
    }

    private IFileInfo GetFileInfo<T>(IEntityId<T> id) where T : struct, IEntity
    {
        const string fileName = "server-config.yml";
        string path = id switch
        {
            ClientId clientId => Path.Combine("clients", clientId.ToString(), fileName),
            InstanceId instanceId => Path.Combine("instances", instanceId.ToString(), fileName),
            _ => throw new TypeSwitchException<IEntityId<T>>(id)
        };
        return _fileProvider.GetFileInfo(path);
    }
}
