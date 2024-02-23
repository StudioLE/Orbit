namespace Orbit.Schema;

/// <summary>
/// The schema for the operating system of an <see cref="Instance"/>
/// </summary>
// ReSharper disable once InconsistentNaming
public readonly record struct OS()
{
    /// <summary>
    /// The name of the operating system.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The version of the operating system.
    /// </summary>
    public string Version { get; init; } = string.Empty;
}
