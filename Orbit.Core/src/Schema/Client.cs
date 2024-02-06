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
    /// The names of the network the client is connected to.
    /// </summary>
    [Required]
    public string[] Networks { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The WireGuard configuration of the client.
    /// </summary>
    [ValidateComplexType]
    public WireGuardClient[] WireGuard { get; set; } = Array.Empty<WireGuardClient>();
}
