using System.ComponentModel.DataAnnotations;

namespace Orbit.Utils.CommandLine;

/// <summary>
/// Options for a secure shell connection.
/// </summary>
public class SshOptions
{
    public const string SectionKey = "SSH";

    /// <summary>
    /// The SSH host.
    /// </summary>
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
