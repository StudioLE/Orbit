using Microsoft.Extensions.FileProviders;
using Orbit.Core.Schema;

namespace Orbit.Core.Providers;

public class ClusterProvider
{
    private readonly IFileProvider _provider;

    public ClusterProvider(IFileProvider provider)
    {
        _provider = provider;
    }

    public Cluster? Get(string clusterName)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(clusterName, clusterName + ".yml"));
        return file.ReadYamlFileAs<Cluster>();
    }

    public bool Put(Cluster cluster)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(cluster.Name, cluster.Name + ".yml"));
        return file.WriteYamlFile(cluster);
    }

    public IEnumerable<string> GetAllNames()
    {
        return _provider.GetDirectoryContents(string.Empty)
            .Where(x => x.IsDirectory)
            .Select(x => x.Name);
    }

    public IEnumerable<Cluster> GetAll()
    {
        return GetAllNames()
            .Select(Get)
            .OfType<Cluster>();
    }
}
