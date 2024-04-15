using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;

namespace Orbit.CloudInit;

/// <summary>
/// Provides the cloud-init user config.
/// </summary>
public class UserConfigProvider
{
    private readonly IEntityFileProvider _fileProvider;

    /// <summary>
    /// DI constructor for <see cref="UserConfigProvider"/>.
    /// </summary>
    public UserConfigProvider(IEntityFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    /// <summary>
    /// Retrieve the cloud-init user config yaml for an instance.
    /// </summary>
    /// <param name="id">The instance id.</param>
    /// <returns>The cloud-init user config yaml, or <see langword="null"/> if it doesn't exist.</returns>
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
    /// Store the cloud-init user config yaml for an instance.
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
        string path = Path.Combine("instances", "artifacts", id.ToString(), "lxd-config.yml");
        return _fileProvider.GetFileInfo(path);
    }
}
