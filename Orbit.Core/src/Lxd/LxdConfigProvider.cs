using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;

namespace Orbit.Lxd;

/// <summary>
/// Provides the LXD configuration yaml for a virtual machine instance.
/// </summary>
public class LxdConfigProvider
{
    private readonly IEntityFileProvider _fileProvider;

    /// <summary>
    /// DI constructor for <see cref="LxdConfigProvider"/>.
    /// </summary>
    public LxdConfigProvider(IEntityFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    /// <summary>
    /// Retrieve the LXD configuration yaml for a virtual machine instance.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <returns>The LXD configuration yaml, or <see langword="null"/> if it doesn't exist.</returns>
    public string? Get(InstanceId id)
    {
        IFileInfo file = GetFileInfo(id);
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Store the LXD configuration yaml for a virtual machine instance.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="content"></param>
    /// <returns><see langword="true"/> if the configuration is stored successfully, otherwise <see langword="false"/>.</returns>
    /// <exception cref="Exception">Thrown if the file provider is not physical.</exception>
    public bool Put(InstanceId id, string content)
    {
        IFileInfo file = GetFileInfo(id);
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if (!directory.Exists)
            directory.Create();
        using StreamWriter writer = physicalFile.CreateText();
        writer.Write(content);
        return true;
    }

    private IFileInfo GetFileInfo(InstanceId id)
    {
        string path = Path.Combine("instances", id.ToString(), "lxd-config.yml");
        return _fileProvider.GetFileInfo(path);
    }
}
