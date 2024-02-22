using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Provision;

public class EntityFileProvider : IEntityFileProvider
{
    private readonly IFileProvider? _fileProvider;

    /// <summary>
    /// DI constructor for <see cref="EntityFileProvider"/>.
    /// </summary>
    public EntityFileProvider(ILogger<EntityFileProvider> logger, IOptions<ProviderOptions> options)
    {
        ProviderOptions providerOptions = options.Value;
        if (!providerOptions.TryValidate(logger))
            return;
        _fileProvider = new PhysicalFileProvider(providerOptions.Directory);
    }

    /// <inheritdoc/>
    public IFileInfo GetFileInfo(string subpath)
    {
        if (_fileProvider is null)
            throw new("File provider is null.");
        return _fileProvider.GetFileInfo(subpath);
    }

    /// <inheritdoc/>
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
        if (_fileProvider is null)
            throw new("File provider is null.");
        return _fileProvider.GetDirectoryContents(subpath);
    }

    /// <inheritdoc/>
    public IChangeToken Watch(string filter)
    {
        if (_fileProvider is null)
            throw new("File provider is null.");
        return _fileProvider.Watch(filter);
    }

    public static string GetDirectory<T>() where T : IEntity
    {
        Type type = typeof(T);
        if (type == typeof(Instance))
            return InstanceId.Directory;
        if (type == typeof(Server))
            return ServerId.Directory;
        if (type == typeof(Client))
            return ClientId.Directory;
        throw new("Invalid entity type.");
    }
}
