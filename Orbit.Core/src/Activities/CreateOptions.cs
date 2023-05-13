using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Activities;

/// <summary>
/// Configuration options for <see cref="Create"/>.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0#bind-hierarchical-configuration-data-using-the-options-pattern">Options pattern</see>
public class CreateOptions : IHasValidationAttributes
{
    private const string MarkerKey = "Create";

    /// <summary>
    /// The name to give the user with sudo privileges.
    /// </summary>
    [Required]
    public string SudoUser { get; set; } = string.Empty;

    /// <summary>
    /// The name to give the user without sudo privileges.
    /// </summary>
    [Required]
    public string User { get; set; } = string.Empty;

    /// <summary>
    /// Public SSH keys authorized to sign in as either user.
    /// </summary>
    public string[] SshAuthorizedKeys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// DI constructor for <see cref="CreateOptions"/>.
    /// </summary>
    public CreateOptions(IConfiguration configuration)
    {
        configuration.GetSection(MarkerKey).Bind(this);
    }
}
