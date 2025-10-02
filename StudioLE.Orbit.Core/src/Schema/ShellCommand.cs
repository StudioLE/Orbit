namespace StudioLE.Orbit.Schema;

/// <summary>
/// A shell command to configure a <see cref="Server"/>.
/// </summary>
public record struct ShellCommand()
{
    /// <summary>
    /// The shell command to be executed on a <see cref="Server"/>.
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// The message to display on success.
    /// </summary>
    public string? SuccessMessage { get; set; } = null;

    /// <summary>
    /// The message to display on failure.
    /// </summary>
    public string? ErrorMessage { get; set; } = null;
}
