namespace StudioLE.CommandLine.Utils.Patterns;

/// <summary>
/// Build a <typeparamref name="TResult"/> from a <typeparamref name="TSource"/>
/// using a <see href="https://refactoring.guru/design-patterns/strategy">strategy pattern</see>.
/// </summary>
public interface IStrategy<in TSource, out TResult>
{
    public TResult Execute(TSource source);
}
