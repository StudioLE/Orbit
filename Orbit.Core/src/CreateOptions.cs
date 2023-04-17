using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils;
using StudioLE.Core.System;

namespace Orbit.Core;

public class ProviderOptions
{
    private const string MarkerKey = "Provider";

    [Required]
    public string Directory { get; set; } = string.Empty;

    public ProviderOptions()
    {
    }

    public ProviderOptions(IConfiguration configuration)
    {
        configuration.GetSection(MarkerKey).Bind(this);
        if(!this.TryValidate(out IReadOnlyCollection<string> errors))
            throw new(errors.Join());
        if (!System.IO.Directory.Exists(Directory))
            throw new("The directory does not exist");
    }
}
