using Microsoft.Extensions.FileProviders;
using Orbit.Core.Schema;

namespace Orbit.Core.Providers;

/// <summary>
/// A repository of <see cref="Cluster"/>.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design">Repository pattern</see>.
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
