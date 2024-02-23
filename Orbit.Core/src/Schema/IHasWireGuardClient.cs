namespace Orbit.Schema;

/// <summary>
/// The schema for a WireGuard client.
/// </summary>
public interface IHasWireGuardClient : IEntity
{
    /// <summary>
    /// The names of the servers the entity is connected to.
    /// </summary>
    public string[] Connections { get; init; }

    /// <summary>
    /// The network interfaces of the entity.
    /// </summary>
    public Interface[] Interfaces { get; init; }

    /// <summary>
    /// The WireGuard configuration of the entity.
    /// </summary>
    public WireGuardClient[] WireGuard { get; init; }
}
