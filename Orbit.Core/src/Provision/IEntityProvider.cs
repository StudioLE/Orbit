using Orbit.Schema;
using Orbit.Utils.Patterns;

namespace Orbit.Provision;

public interface IEntityProvider<T> : IProvider<IEntityId<T>, T> where T : struct, IEntity
{
    /// <summary>
    /// Get the <typeparamref name="T"/> for the <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="value">The value.</param>
    public bool Put(IEntityId<T> id, T value);

    public IEnumerable<string> GetIndex();

    bool PutArtifact(IEntityId<T> id, string fileName, string content);

    string? GetArtifact(IEntityId<T> id, string fileName);
}
