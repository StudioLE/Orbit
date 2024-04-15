using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Provision;

/// <summary>
/// A concrete implementation of <see cref="IEntityFileProvider"/> that stores entities as files using a
/// <see cref="PhysicalFileProvider"/>.
/// </summary>
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
}
