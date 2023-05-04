using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core;

public class ProviderOptions : IHasValidationAttributes
{
    private const string MarkerKey = "Provider";
    private string? _directory;

    // TODO: Add a directory exists CustomValidationAttribute
    public string Directory
    {
        get => string.IsNullOrEmpty(_directory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".orbit")
            : _directory;
        set => _directory = value;
    }

    public ProviderOptions()
    {
    }

    public ProviderOptions(IConfiguration configuration) : this()
    {
        configuration.GetSection(MarkerKey).Bind(this);
    }
}
