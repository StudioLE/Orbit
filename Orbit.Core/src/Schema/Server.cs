using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the server hosting an <see cref="Instance"/>.
/// </summary>
public readonly record struct Server() : IEntity, IHasValidationAttributes
{
    /// <summary>
    /// The name of the server.
    /// </summary>
    [NameSchema]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The number of the server.
    /// </summary>
    [Range(1, 64)]
    public int Number { get; init; } = 0;

    /// <summary>
    /// The interfaces connecting the server to a network.
    /// </summary>
    public Interface[] Interfaces { get; init; } = Array.Empty<Interface>();

    /// <summary>
    /// The SSH connection details for the server.
    /// </summary>
    [ValidateComplexType]
    public SshConnection Ssh { get; init; } = new();

    /// <summary>
    /// The WireGuard configuration for the server.
    /// </summary>
    [ValidateComplexType]
    public WireGuardServer WireGuard { get; init; } = new();
}
