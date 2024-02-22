using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for an instance.
/// </summary>
public sealed class Instance : IHasWireGuardClient, IEntity, IHasValidationAttributes
{
    /// <summary>
    /// The name of the instance.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The number of the instance.
    /// </summary>
    [Range(1, 64)]
    public int Number { get; set; }

    /// <summary>
    /// The role of the instance.
    /// </summary>
    [NameSchema]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The name of the server hosting the instance.
    /// </summary>
    [Required]
    [NameSchema]
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// The names of the networks the instance is connected to.
    /// </summary>
    [Required]
    public string[] Connections { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The network interfaces of the instance.
    /// </summary>
    [Required]
    public Interface[] Interfaces { get; set; } = Array.Empty<Interface>();

    /// <summary>
    /// The operating system of the instance.
    /// </summary>
    [ValidateComplexType]
    // ReSharper disable once InconsistentNaming
    public OS OS { get; set; } = new();

    /// <summary>
    /// The hardware of the instance.
    /// </summary>
    [ValidateComplexType]
    public Hardware Hardware { get; set; } = new();

    /// <summary>
    /// The WireGuard configuration of the instance.
    /// </summary>
    [ValidateComplexType]
    public WireGuardClient[] WireGuard { get; set; } = Array.Empty<WireGuardClient>();

    /// <summary>
    /// The domain names to be reverse proxied to the instance.
    /// </summary>
    [Required]
    public string[] Domains { get; set; } = Array.Empty<string>();

    // TODO: Remove Mounts
    /// <summary>
    /// The mounted directories of the instance.
    /// </summary>
    public Mount[] Mounts { get; set; } = Array.Empty<Mount>();

    /// <summary>
    /// The packages to install.
    /// </summary>
    public string[] Install { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The packages to run.
    /// </summary>
    public string[] Run { get; set; } = Array.Empty<string>();
}
