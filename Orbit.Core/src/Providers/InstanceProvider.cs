using Microsoft.Extensions.FileProviders;
using Orbit.Core.Schema;

namespace Orbit.Core.Providers;

public class InstanceProvider
{
    private readonly IFileProvider _provider;

    public InstanceProvider(IFileProvider provider)
    {
        _provider = provider;
    }

    public Instance? Get(string clusterName, string instanceName)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(clusterName, instanceName, instanceName + ".yml"));
        return file.ReadYamlFileAs<Instance>();
    }

    public bool Put(Instance instance)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(instance.Cluster, instance.Name, instance.Name + ".yml"));
        return file.WriteYamlFile(instance);
    }

    public bool PutResource(string clusterName, string instanceName, string fileName, object obj)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(clusterName, instanceName, fileName));
        return file.WriteYamlFile(obj, "#cloud-config" + Environment.NewLine);
    }

    public bool PutResource(string clusterName, string instanceName, string fileName, string content)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(clusterName, instanceName, fileName));
        return file.WriteTextFile(content);
    }

    public string? GetResource(string clusterName, string instanceName, string fileName)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(clusterName, instanceName, fileName));
        return file.ReadTextFile();
    }

    public IEnumerable<string> GetAllNamesInCluster(string clusterName)
    {
        return _provider.GetDirectoryContents(clusterName)
            .Where(x => x.IsDirectory)
            .Select(x => x.Name);
    }

    public IEnumerable<Instance> GetAllInCluster(string clusterName)
    {
        return GetAllNamesInCluster(clusterName)
            .Select(instanceName => Get(clusterName, instanceName))
            .OfType<Instance>();
    }
}
