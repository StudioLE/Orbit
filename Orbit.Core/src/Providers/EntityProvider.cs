using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Logging;

namespace Orbit.Core.Providers;

public class EntityProvider
{
    public bool IsValid { get; }

    public InstanceProvider Instance { get; } = null!;

    public ClusterProvider Cluster { get; } = null!;

    public ServerProvider Server { get; } = null!;

    public EntityProvider(ProviderOptions options, ILogger<EntityProvider> logger)
    {
        if (!options.TryValidate(logger))
            return;
        IsValid = true;
        PhysicalFileProvider provider = new(options.Directory);
        Instance = new(provider);
        Cluster = new(provider);
        Server = new(provider);
    }

    public static EntityProvider CreateTemp()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        ILogger<EntityProvider> logger = LoggingHelpers.CreateConsoleLogger<EntityProvider>();
        EntityProvider provider = new(new()
            {
                Directory = directory
            },
            logger);
        provider.Server.Put(new()
        {
            Name = "server-01",
            Number = 1,
            Address = "localhost",
            Ssh = new()
            {
                Port = 22,
                User = "user",
                PrivateKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh/id_rsa")
            }
        });
        provider.Cluster.Put(new()
        {
            Name = "cluster-01",
            Number = 1
        });
        return provider;
    }
}
