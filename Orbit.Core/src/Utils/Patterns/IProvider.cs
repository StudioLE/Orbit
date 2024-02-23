namespace Orbit.Utils.Patterns;

/// <summary>
/// A service to provide a <typeparamref name="TValue"/> for a give <typeparamref name="TKey"/>.
/// </summary>
/// <remarks>
/// Follows a <see href="https://www.patterns.dev/posts/provider-pattern">provider design pattern</see>.
/// </remarks>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the provided value.</typeparam>
/// <seealso href="https://www.patterns.dev/posts/provider-pattern"/>
public interface IProvider<in TKey, TValue> where TValue : struct
{
    /// <summary>
    /// Get the <typeparamref name="TValue"/> for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The <typeparamref name="TValue"/>.</returns>
    public TValue? Get(TKey key);
}
