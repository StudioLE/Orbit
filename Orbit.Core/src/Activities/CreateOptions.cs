using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
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
        ValidationContext context = new(this);
        List<ValidationResult> results = new();
        if (!Validator.TryValidateObject(this, context, results))
            throw new(results.Select(x => x.ErrorMessage).OfType<string>().Join());
    }
}
