using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Orbit.Core.Schema;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Logging;
using YamlDotNet.Serialization;

namespace Orbit.Core;

public class InstanceProvider
{
    private readonly IFileProvider _provider = null!;

    public bool IsValid { get; } = false;

    public InstanceProvider(ProviderOptions options, ILogger<InstanceProvider> logger)
    {
        if(!options.TryValidate(logger))
            return;
        IsValid = true;
        _provider = new PhysicalFileProvider(options.Directory);
    }

    public static InstanceProvider CreateTemp()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        ILogger<InstanceProvider> logger = LoggingHelpers.CreateConsoleLogger<InstanceProvider>();
        return new(new()
        {
            Directory = directory
        }, logger);
    }

    public IEnumerable<string> GetAllIds()
    {
        return _provider.GetDirectoryContents(string.Empty)
            .Where(x => x.IsDirectory)
            .Select(x => x.Name);
    }

    public Instance? Get(string instanceId)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(instanceId, instanceId + ".yml"));
        if (!file.Exists)
            return null;
        Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        IDeserializer deserializer = Yaml.Deserializer();
        return deserializer.Deserialize<Instance>(reader);
    }

    public bool Put(Instance instance)
    {
        return PutResource(instance.Id, instance.Id + ".yml", instance);
    }

    public bool PutResource(string instanceId, string fileName, object obj)
    {
        IFileInfo providerFile = _provider.GetFileInfo(Path.Combine(instanceId, fileName));
        if(providerFile.Exists)
            return false;
        FileInfo physicalFile = new(providerFile.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if(!directory.Exists)
            directory.Create();
        if(providerFile.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        writer.WriteLine("#cloud-config");
        writer.WriteLine();
        ISerializer serializer = Yaml.Serializer();
        serializer.Serialize(writer, obj);
        return true;
    }
}
