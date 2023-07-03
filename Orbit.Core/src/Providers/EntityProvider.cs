using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Logging;

namespace Orbit.Core.Providers;

/// <summary>
/// A repository of <see cref="Instance"/>, and <see cref="Server"/>.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design">Repository pattern</see>
/// .
public class EntityProvider
{
    public bool IsValid { get; }

    public InstanceProvider Instance { get; } = null!;

    public ServerProvider Server { get; } = null!;

    public EntityProvider(IOptions<ProviderOptions> options, ILogger<EntityProvider> logger)
    {
        ProviderOptions providerOptions = options.Value;

        if (!providerOptions.TryValidate(logger))
            return;
        IsValid = true;
        PhysicalFileProvider provider = new(providerOptions.Directory);
        Instance = new(provider);
        Server = new(provider);
    }

    public static EntityProvider CreateTemp()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(directory);
        ILogger<EntityProvider> logger = LoggingHelpers.CreateConsoleLogger<EntityProvider>();
        OptionsWrapper<ProviderOptions> options = new(new()
        {
            Directory = directory
        });
        EntityProvider provider = new(options, logger);
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
        return provider;
    }
}
