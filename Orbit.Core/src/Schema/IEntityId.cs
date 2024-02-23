namespace Orbit.Schema;

// ReSharper disable once UnusedTypeParameter
public interface IEntityId<T> where T : IEntity
{
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string Name { get; init; }

    public string GetFilePath();
}
