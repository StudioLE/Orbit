using System.ComponentModel.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the SSH connection of a <see cref="Server"/>.
/// </summary>
public class SshConnection : IHasValidationAttributes
{
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

    /// <summary>
    /// The optional SSH password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The optional path to an SSH private key.
    /// </summary>
    public string PrivateKeyFile { get; set; } = string.Empty;
}
