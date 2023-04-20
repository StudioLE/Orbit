using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Logging;

namespace Orbit.Core.Providers;

public class EntityProvider
{
    public bool IsValid { get; } = false;

    public InstanceProvider Instance { get; } = null!;

    public ClusterProvider Cluster { get; } = null!;

    public HostProvider Host { get; } = null!;

    public EntityProvider(ProviderOptions options, ILogger<EntityProvider> logger)
    {
        if(!options.TryValidate(logger))
            return;
        IsValid = true;
        PhysicalFileProvider provider = new (options.Directory);
        Instance = new(provider);
        Cluster = new(provider);
        Host = new(provider);
    }

    public static EntityProvider CreateTemp()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        ILogger<EntityProvider> logger = LoggingHelpers.CreateConsoleLogger<EntityProvider>();
        return new(new()
        {
            Directory = directory
        }, logger);
    }
}
