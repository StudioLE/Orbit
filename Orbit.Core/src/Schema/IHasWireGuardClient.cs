namespace Orbit.Schema;

/// <summary>
/// The schema for a WireGuard client.
/// </summary>
public interface IHasWireGuardClient : IEntity
{
    /// <summary>
    /// The names of the network the instance is connected to.
    /// </summary>
    public string[] Networks { get; }

    /// <summary>
    /// The WireGuard configuration of the instance.
    /// </summary>
    public WireGuardClient[] WireGuard { get; set; }
}
