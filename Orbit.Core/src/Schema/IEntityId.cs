namespace Orbit.Schema;

// ReSharper disable once UnusedTypeParameter
public interface IEntityId<T> where T : IEntity
{
    public bool IsDefault();
}
