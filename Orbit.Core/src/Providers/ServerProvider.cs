using Microsoft.Extensions.FileProviders;
using Orbit.Core.Schema;

namespace Orbit.Core.Providers;


/// <summary>
/// A repository of <see cref="Server"/>.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design">Repository pattern</see>.
public class ServerProvider
{
    private const string Directory = "servers";
    private readonly IFileProvider _provider;

    public ServerProvider(IFileProvider provider)
    {
        _provider = provider;
    }

    public Server? Get(string serverName)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(Directory, serverName + ".yml"));
        return file.ReadYamlFileAs<Server>();
    }

    public bool Put(Server server)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(Directory, server.Name + ".yml"));
        return file.WriteYamlFile(server);
    }

    public IEnumerable<string> GetAllNames()
    {
        return _provider.GetDirectoryContents(Directory)
            .Where(x => !x.IsDirectory
                        && x.Name.EndsWith(".yml"))
            .Select(x => x.Name.Substring(0, x.Name.Length - 4));
    }

    public IEnumerable<Server> GetAll()
    {
        return GetAllNames()
            .Select(Get)
            .OfType<Server>();
    }
}
