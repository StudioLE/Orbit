using Orbit.Clients;
using Orbit.Schema;
using StudioLE.Storage.Files;

namespace Orbit.WireGuard;

/// <summary>
/// Provides the WireGuard configuration for a <see cref="Client"/>.
/// </summary>
public class WireGuardConfigProvider
{
    private readonly IFileReader _reader;
    private readonly IFileWriter _writer;

    /// <summary>
    /// DI constructor for <see cref="WireGuardConfigProvider"/>.
    /// </summary>
    public WireGuardConfigProvider(IFileReader reader, IFileWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    /// <summary>
    /// Retrieve the WireGuard configuration.
    /// </summary>
    /// <param name="id">The client id.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The WireGuard configuration, or <see langword="null"/> if it doesn't exist.</returns>
    public async Task<string?> Get(ClientId id, string fileName)
    {
        string path = GetFilePath(id, fileName);
        await using Stream? stream = await _reader.Read(path);
        if (stream is null)
            return null;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }

    /// <summary>
    /// Store the WireGuard configuration
    /// </summary>
    /// <param name="id">The client id.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="config">The WireGuard configuration.</param>
    /// <returns><see langword="true"/> if the configuration is stored successfully, otherwise <see langword="false"/>.</returns>
    /// <exception cref="Exception">Thrown if the file provider is not physical.</exception>
    public async Task<bool> Put(ClientId id, string fileName, string config)
    {
        string path = GetFilePath(id, fileName);
        await using Stream? stream = await _writer.OpenWrite(path, out string uri);
        if (stream is null)
            return false;
        await using StreamWriter writer = new(stream);
        await writer.WriteAsync(config);
        return !string.IsNullOrEmpty(uri);
    }

    private static string GetFilePath(ClientId id, string fileName)
    {
        return Path.Combine(ClientProvider.DirectoryName, id.ToString(), fileName);
    }
}
