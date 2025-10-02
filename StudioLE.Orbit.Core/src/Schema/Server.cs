using System.ComponentModel.DataAnnotations;
using StudioLE.Orbit.Schema.DataAnnotations;
using StudioLE.Orbit.Utils.DataAnnotations;

namespace StudioLE.Orbit.Schema;

/// <summary>
/// The schema for the server hosting an <see cref="Instance"/>.
/// </summary>
public record struct Server() : IEntity, IHasValidationAttributes
{
    /// <summary>
    /// The name of the server.
    /// </summary>
    [NameSchema]
    public ServerId Name { get; set; } = new();

    /// <summary>
    /// The number of the server.
    /// </summary>
    [Range(1, 64)]
    public int Number { get; set; } = 0;

    /// <summary>
    /// The interfaces connecting the server to a network.
    /// </summary>
    public Interface[] Interfaces { get; set; } = Array.Empty<Interface>();

    /// <summary>
    /// The SSH connection details for the server.
    /// </summary>
    [ValidateComplexType]
    public SshConnection Ssh { get; set; } = new();

    /// <summary>
    /// The WireGuard configuration for the server.
    /// </summary>
    [ValidateComplexType]
    public WireGuardServer WireGuard { get; set; } = new();
}
