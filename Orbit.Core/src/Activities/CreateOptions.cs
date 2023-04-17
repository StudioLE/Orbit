using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Activities;

public class CreateOptions : IHasValidationAttributes
{
    private const string MarkerKey = "Create";

    [Required]
    public string SudoUser { get; set; } = string.Empty;

    [Required]
    public string User { get; set; } = string.Empty;

    public string[] SshAuthorizedKeys { get; set; } = Array.Empty<string>();

    public CreateOptions(IConfiguration configuration)
    {
        configuration.GetSection(MarkerKey).Bind(this);
    }
}
