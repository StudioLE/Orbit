using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core;

/// <summary>
/// Configuration options for <see cref="Providers.EntityProvider"/>.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0#bind-hierarchical-configuration-data-using-the-options-pattern">Options pattern</see>
public class ProviderOptions : IHasValidationAttributes
{
    private const string MarkerKey = "Provider";
    private string? _directory;

    /// <summary>
    /// The directory to store the entities in.
    /// </summary>
    // TODO: Add a directory exists CustomValidationAttribute
    public string Directory
    {
        get => string.IsNullOrEmpty(_directory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".orbit")
            : _directory;
        set => _directory = value;
    }

    /// <summary>
    /// Construct <see cref="ProviderOptions"/>.
    /// </summary>
    public ProviderOptions()
    {
    }

    /// <summary>
    /// DI constructor for <see cref="ProviderOptions"/>.
    /// </summary>
    public ProviderOptions(IConfiguration configuration) : this()
    {
        configuration.GetSection(MarkerKey).Bind(this);
    }
}
