using Orbit.Instances;
using Orbit.Schema;
using StudioLE.Storage.Files;

namespace Orbit.Lxd;

/// <summary>
/// Provide and store the LXD configuration.
/// </summary>
public class LxdConfigProvider
{
    internal const string FileName = "lxd-config.yml";
    private readonly IFileReader _reader;
    private readonly IFileWriter _writer;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigProvider"/>.
    /// </summary>
    public LxdConfigProvider(IFileReader reader, IFileWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    /// <summary>
    /// Retrieve the LXD configuration.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <returns>The LXD configuration, or <see langword="null"/> if it doesn't exist.</returns>
    public async Task<string?> Get(InstanceId id)
    {
        await using Stream? stream = await _reader.Read(GetFilePath(id));
        if (stream is null)
            return null;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Store the LXD configuration.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <param name="config">The LXD configuraiton as YAML</param>
    /// <returns><see langword="true"/> if the configuration is stored successfully, otherwise <see langword="false"/>.</returns>
    public async Task<bool> Put(InstanceId id, string config)
    {
        string path = GetFilePath(id);
        await using Stream? stream = await _writer.OpenWrite(path, out string uri);
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
