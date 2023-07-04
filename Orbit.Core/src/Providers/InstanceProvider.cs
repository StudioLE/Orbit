using Microsoft.Extensions.FileProviders;
using Orbit.Core.Schema;

namespace Orbit.Core.Providers;

/// <summary>
/// A repository of <see cref="Instance"/>.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design">Repository pattern</see>.
public class InstanceProvider
{
    private const string Directory = "instances";
    private readonly IFileProvider _provider;

    public InstanceProvider(IFileProvider provider)
    {
        _provider = provider;
    }

    public Instance? Get(string instanceName)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(Directory, instanceName, instanceName + ".yml"));
        return file.ReadYamlFileAs<Instance>();
    }

    public bool Put(Instance instance)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(Directory, instance.Name, instance.Name + ".yml"));
        return file.WriteYamlFile(instance);
    }

    public bool PutResource(string instanceName, string fileName, object obj)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(Directory, instanceName, fileName));
        return file.WriteYamlFile(obj, "#cloud-config" + Environment.NewLine);
    }

    public string? GetResource(string instanceName, string fileName)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(Directory, instanceName, fileName));
        return file.ReadTextFile();
    }

    public IEnumerable<string> GetAllNames()
    {
        return _provider.GetDirectoryContents(Directory)
            .Where(x => x.IsDirectory)
            .Select(x => x.Name);
    }

    public IEnumerable<Instance> GetAll()
    {
        return GetAllNames()
            .Select(Get)
            .OfType<Instance>();
    }
}
