using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core;

public class ProviderOptions : IHasValidationAttributes
{
    private const string MarkerKey = "Provider";

    // TODO: Add a directory exists CustomValidationAttribute
    [Required]
    public string Directory { get; set; } = string.Empty;

    public ProviderOptions()
    {
    }

    public ProviderOptions(IConfiguration configuration)
    {
        configuration.GetSection(MarkerKey).Bind(this);
    }
}
