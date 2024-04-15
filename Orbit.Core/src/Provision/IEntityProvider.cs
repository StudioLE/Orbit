using Orbit.Schema;
using Orbit.Utils.Patterns;

namespace Orbit.Provision;

/// <summary>
/// A provider for entities.
/// </summary>
/// <typeparam name="T">The type of entity to provide.</typeparam>
public interface IEntityProvider<T> : IProvider<IEntityId<T>, T> where T : struct, IEntity
{
    /// <summary>
    /// Get the <typeparamref name="T"/> for the <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="value">The value.</param>
    public bool Put(IEntityId<T> id, T value);

    /// <summary>
    /// Get the id of all stored entities.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetIndex();
}
