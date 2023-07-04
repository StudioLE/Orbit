using Orbit.Core.Schema;
using Orbit.Core.Utils.Patterns;

namespace Orbit.Core.Provision;

public interface IEntityProvider<T> : IProvider<IEntityId<T>, T?> where T : class, IEntity
{
    /// <summary>
    /// Get the <typeparamref name="T"/> for the <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="value">The value.</param>
    public bool Put(IEntityId<T> id, T value);

    public IEnumerable<string> GetIndex();

    bool PutResource(IEntityId<T> id, string fileName, object obj, string? prefixYaml = null);

    string? GetResource(IEntityId<T> id, string fileName);
}
