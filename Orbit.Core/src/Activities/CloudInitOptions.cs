using System.ComponentModel.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Activities;

/// <summary>
/// Configuration options for <see cref="Generate"/>.
/// </summary>
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0#bind-hierarchical-configuration-data-using-the-options-pattern">Options pattern</see>
public class CloudInitOptions : IHasValidationAttributes
{
    /// <summary>
    /// The
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-7.0#bind-hierarchical-configuration-data-using-the-options-pattern">options pattern</see> key.
    /// </summary>
    public const string SectionKey = "CloudInit";

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
}
