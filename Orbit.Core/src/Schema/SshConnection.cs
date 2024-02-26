using System.ComponentModel.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the SSH connection of a <see cref="Server"/>.
/// </summary>
public record struct SshConnection() : IHasValidationAttributes
{
    /// <summary>
    /// The SSH host.
    /// </summary>
    /// <remarks>
    /// This could be an IP address or a domain name.
    /// </remarks>
    [Required]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// The SSH port.
    /// </summary>
    [Required]
    [Range(0, 65536)]
    public int Port { get; set; } = 22;

    /// <summary>
    /// The SSH user.
    /// </summary>
    [Required]
    public string User { get; set; } = string.Empty;


}
