using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for a client.
/// </summary>
public sealed class Client : IEntity, IHasWireGuardClient, IHasValidationAttributes
{
    /// <summary>
    /// The name of the client.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The number of the client.
    /// </summary>
    [Range(100, 150)]
    public int Number { get; set; }

    /// <summary>
    /// The names of the servers the client has WireGuard connections to.
    /// </summary>
    [Required]
    public string[] Connections { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    public Interface[] Interfaces { get; set; } = Array.Empty<Interface>();

    /// <summary>
    /// The WireGuard configuration of the client.
    /// </summary>
    [ValidateComplexType]
    public WireGuardClient[] WireGuard { get; set; } = Array.Empty<WireGuardClient>();
}
