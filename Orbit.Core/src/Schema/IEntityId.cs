namespace Orbit.Core.Schema;

// ReSharper disable once UnusedTypeParameter
public interface IEntityId<T> where T : class, IEntity
{
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string Name { get; }

    public string GetFilePath();
}
