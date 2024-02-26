namespace Orbit.Schema;

public interface IEntity
{
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The number of the entity.
    /// </summary>
    public int Number { get; set; }
}
