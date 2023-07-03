namespace Orbit.Core.Utils.Patterns;

/// <summary>
/// A service to provide a <typeparamref name="TValue"/> for a give <typeparamref name="TKey"/> asynchronously.
/// </summary>
/// <remarks>
/// Follows a <see href="https://www.patterns.dev/posts/provider-pattern">provider design pattern</see>.
/// </remarks>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the provided value.</typeparam>
/// <seealso href="https://www.patterns.dev/posts/provider-pattern"/>
public interface IAsyncProvider<in TKey, TValue> : IProvider<TKey, Task<TValue>>
{
}
