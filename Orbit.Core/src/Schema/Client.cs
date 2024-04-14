using System.ComponentModel.DataAnnotations;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for a client.
/// </summary>
public record struct Client() : IEntity, IHasWireGuardClient, IHasValidationAttributes
{
    /// <summary>
    /// The name of the client.
    /// </summary>
    [NameSchema]
    public ClientId Name { get; set; } = new();

    /// <summary>
    /// The number of the client.
    /// </summary>
    [Range(100, 150)]
    public int Number { get; set; } = 0;

    /// <summary>
    /// The names of the servers the client has WireGuard connections to.
    /// </summary>
    [Required]
    public ServerId[] Connections { get; set; } = Array.Empty<ServerId>();

    /// <summary>
    /// The WireGuard configuration of the client.
    /// </summary>
    [ValidateComplexType]
    public WireGuardClient[] WireGuard { get; set; } = Array.Empty<WireGuardClient>();
}
