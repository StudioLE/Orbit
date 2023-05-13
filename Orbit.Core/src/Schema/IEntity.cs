using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

public interface IEntity
{
    /// <summary>
    /// The name of the entity.
    /// </summary>
    [NameSchema]
    public string Name { get; }

    /// <summary>
    /// The number of the entity.
    /// </summary>
    [Range(1, 64)]
    public int Number { get; }
}
