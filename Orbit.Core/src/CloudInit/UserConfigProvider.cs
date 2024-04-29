using Orbit.Instances;
using Orbit.Schema;
using StudioLE.Storage.Files;

namespace Orbit.CloudInit;

/// <summary>
/// Provide and store the cloud-init user config.
/// </summary>
public class UserConfigProvider
{
    internal const string FileName = "user-config.yml";
    private readonly IFileReader _reader;
    private readonly IFileWriter _writer;

    /// <summary>
    /// DI constructor for <see cref="UserConfigProvider"/>.
    /// </summary>
    public UserConfigProvider(IFileReader reader, IFileWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    /// <summary>
    /// Retrieve the cloud-init user config.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <returns>The cloud-init user config, or <see langword="null"/> if it doesn't exist.</returns>
    public async Task<string?> Get(InstanceId id)
    {
        string path = GetFilePath(id);
        await using Stream? stream = await _reader.Read(path);
        if (stream is null)
            return null;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Store the cloud-init user config.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <param name="config">The cloud-init user config as YAML.</param>
    /// <returns><see langword="true"/> if the configuration is stored successfully, otherwise <see langword="false"/>.</returns>
    public async Task<bool> Put(InstanceId id, string config)
    {
        string path = GetFilePath(id);
        await using Stream? stream = await _writer.Open(path, out string uri);
        if (stream is null)
            return false;
        await using StreamWriter writer = new(stream);
        await writer.WriteAsync(config);
        return !string.IsNullOrEmpty(uri);
    }

    private static string GetFilePath(InstanceId id)
    {
        return Path.Combine(InstanceProvider.DirectoryName, id.ToString(), FileName);
    }
}
