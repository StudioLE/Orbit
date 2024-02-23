using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the hardware of an <see cref="Instance"/>.
/// </summary>
public record struct Hardware()
{
    /// <summary>
    /// The platform of the hardware.
    /// </summary>
    [EnumDataType(typeof(Platform))]
    public Platform Platform { get; init; } = Platform.Unknown;

    /// <summary>
    /// The type code of the hardware.
    /// </summary>
    ///  TODO: Rename to Code
    [TypeSchema]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The number of CPUs.
    /// </summary>
    [Range(1, 64)]
    public int Cpus { get; init; } = 0;

    /// <summary>
    /// The amount of memory in GB.
    /// </summary>
    [Range(1, 32)]
    public int Memory { get; init; } = 0;

    /// <summary>
    /// The amount of disk space in GB.
    /// </summary>
    [Range(10, 1024)]
    public int Disk { get; init; } = 0;
}
