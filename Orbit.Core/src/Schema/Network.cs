using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for the network configuration of an <see cref="Instance"/>.
/// </summary>
public sealed class Network : IEntity
{
    /// <summary>
    /// The name of the network.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The number of the network.
    /// </summary>
    [Range(1, 64)]
    public int Number { get; set; }

    /// <summary>
    /// The name of the server hosting the network.
    /// </summary>
    [NameSchema]
    public string Server { get; set; } = string.Empty;
}
