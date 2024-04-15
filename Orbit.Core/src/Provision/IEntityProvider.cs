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

    /// <summary>
    /// Store an entity artifact.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    /// <param name="fileName">The file name of the artifact.</param>
    /// <param name="content">The content of the artifact.</param>
    /// <returns>
    /// <see langword="true"/> if the artifact was stored; otherwise <see langword="false"/>.
    /// </returns>
    bool PutArtifact(IEntityId<T> id, string fileName, string content);

    /// <summary>
    /// Retrieve an entity artifact.
    /// </summary>
    /// <param name="id">The id of the entity.</param>
    /// <param name="fileName">The file name of the artifact.</param>
    /// <return>The content of the artifact.</return>
    string? GetArtifact(IEntityId<T> id, string fileName);
}
