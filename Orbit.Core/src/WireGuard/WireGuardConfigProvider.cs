using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;

namespace Orbit.WireGuard;

/// <summary>
/// Provides the WireGuard configuration for a <see cref="Client"/>.
/// </summary>
public class WireGuardConfigProvider
{
    private readonly IEntityFileProvider _fileProvider;

    /// <summary>
    /// DI constructor for <see cref="WireGuardConfigProvider"/>.
    /// </summary>
    public WireGuardConfigProvider(IEntityFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    /// <summary>
    /// Retrieve the WireGuard configuration.
    /// </summary>
    /// <param name="id">The client id.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The WireGuard configuration, or <see langword="null"/> if it doesn't exist.</returns>
    public string? Get(ClientId id, string fileName)
    {
        IFileInfo file = GetFileInfo(id, fileName);
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Store the WireGuard configuration
    /// </summary>
    /// <param name="id">The client id.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="content">The content of the file.</param>
    /// <returns><see langword="true"/> if the configuration is stored successfully, otherwise <see langword="false"/>.</returns>
    /// <exception cref="Exception">Thrown if the file provider is not physical.</exception>
    public bool Put(ClientId id, string fileName, string content)
    {
        IFileInfo file = GetFileInfo(id, fileName);
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        using StreamWriter writer = physicalFile.CreateText();
        writer.Write(content);
        return true;
    }

    private IFileInfo GetFileInfo(ClientId id, string fileName)
    {
        string path = Path.Combine("clients", id.ToString(), fileName);
        return _fileProvider.GetFileInfo(path);
    }
}
