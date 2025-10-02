namespace StudioLE.Orbit.Schema;

/// <summary>
/// The schema for a mount of an <see cref="Instance"/>.
/// </summary>
public record struct Mount()
{
    /// <summary>
    /// The source path on the server.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The target path on the instance.
    /// </summary>
    public string Target { get; set; } = string.Empty;
}
