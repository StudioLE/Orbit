namespace StudioLE.CommandLine.Utils.Patterns;

/// <summary>
/// Build a <typeparamref name="T"/> using a <see href="https://refactoring.guru/design-patterns/builder">builder pattern</see>.
/// </summary>
public interface IBuilder<out T>
{
    /// <inheritdoc cref="IBuilder{T}"/>
    public T Build();
}
