using Microsoft.Extensions.FileProviders;
using Orbit.Core.Schema;

namespace Orbit.Core.Providers;

public class HostProvider
{
    private const string HostsDirectory = "hosts";
    private readonly IFileProvider _provider;

    public HostProvider(IFileProvider provider)
    {
        _provider = provider;
    }

    public Host? Get(string hostName)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(HostsDirectory, hostName + ".yml"));
        return file.ReadYamlFileAs<Host>();
    }

    public bool Put(Host host)
    {
        IFileInfo file = _provider.GetFileInfo(Path.Combine(HostsDirectory, host.Name + ".yml"));
        return file.WriteYamlFile(host);
    }

    public IEnumerable<string> GetAllNames()
    {
        return _provider.GetDirectoryContents(HostsDirectory)
            .Where(x => !x.IsDirectory)
            .Select(x => x.Name);
    }

    public IEnumerable<Host> GetAll()
    {
        return GetAllNames()
            .Select(Get)
            .OfType<Host>();
    }
}
