namespace StudioLE.Orbit.Schema;

/// <summary>
/// The schema for the operating system of an <see cref="Instance"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public record struct OS()
{
    /// <summary>
    /// The name of the operating system.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The version of the operating system.
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
