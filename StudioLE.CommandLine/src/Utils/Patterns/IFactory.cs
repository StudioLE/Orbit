namespace StudioLE.CommandLine.Utils.Patterns;

/// <summary>
/// Build a <typeparamref name="TResult"/> from a <typeparamref name="TSource"/>
/// using a <see href="https://refactoring.guru/design-patterns/factory-method">factory method pattern</see>.
/// </summary>
public interface IFactory<in TSource, out TResult>
{
    public TResult Create(TSource source);
}
