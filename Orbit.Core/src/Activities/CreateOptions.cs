using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils;
using StudioLE.Core.System;

namespace Orbit.Core.Activities;

public class CreateOptions
{
    public const string MarkerKey = "Create";

    [Required]
    public string SudoUser { get; set; } = string.Empty;

    [Required]
    public string User { get; set; } = string.Empty;

    public string[] SshAuthorizedKeys { get; set; } = Array.Empty<string>();

    public CreateOptions()
    {
    }

    public CreateOptions(IConfiguration configuration)
    {
        configuration.GetSection(MarkerKey).Bind(this);
        if(!this.TryValidate(out IReadOnlyCollection<string> errors))
            throw new(errors.Join());
    }
}
