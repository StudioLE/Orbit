namespace StudioLE.Orbit.Schema;

// ReSharper disable once UnusedTypeParameter
/// <summary>
/// The id of an entity.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IEntityId<T> where T : IEntity
{
    /// <summary>
    /// Is the id the default value?
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the id is the default value; otherwise <see langword="false"/>.
    /// </returns>
    public bool IsDefault();
}
