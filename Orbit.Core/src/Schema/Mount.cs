namespace Orbit.Schema;

/// <summary>
/// The schema for a mount of an <see cref="Instance"/>.
/// </summary>
public readonly record struct Mount()
{
    /// <summary>
    /// The source path on the server.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// The target path on the instance.
    /// </summary>
    public string Target { get; init; } = string.Empty;
}
