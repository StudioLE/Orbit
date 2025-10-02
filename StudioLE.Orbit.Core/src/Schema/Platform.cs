namespace StudioLE.Orbit.Schema;

/// <summary>
/// The <see cref="Hardware"/> platform of an <see cref="Instance"/>.
/// </summary>
public enum Platform
{

    /// <summary>
    /// Unknown
    /// </summary>
    Unknown,

    /// <summary>
    /// A bare metal server.
    /// </summary>
    Metal,

    /// <summary>
    /// A virtual machine.
    /// </summary>
    Virtual,

    /// <summary>
    /// A container.
    /// </summary>
    Container
}
