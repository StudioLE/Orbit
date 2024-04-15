using Microsoft.Extensions.FileProviders;
using Orbit.Provision;
using Orbit.Schema;

namespace Orbit.Lxd;

public class LxdConfigProvider
{
    private readonly IEntityFileProvider _fileProvider;

    /// <summary>
    /// DI constructor for <see cref="EntityProvider{T}"/>.
    /// </summary>
    public LxdConfigProvider(IEntityFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public string? Get(InstanceId id)
    {
        IFileInfo file = GetFileInfo(id);
        if (!file.Exists)
            return null;
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

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
