using StudioLE.CommandLine.Composition;
using StudioLE.CommandLine.Utils.Patterns;

namespace StudioLE.CommandLine.Utils.Converters;

/// <summary>
/// Build an <see cref="ConverterResolver"/>.
/// </summary>
public class ConverterResolverBuilder : IBuilder<ConverterResolver>
{
    private readonly Dictionary<Type, Type> _registry = new();

    /// <summary>
    /// Register a converter as resolvable by <see cref="Type"/>.
    /// </summary>
    /// <param name="sourceType">The <see cref="Type"/> to resolve by.</param>
    /// <param name="converter">The resolvable <see cref="ObjectTree"/>.</param>
    /// <returns>The builder.</returns>
    public ConverterResolverBuilder Register(Type sourceType, Type converter)
    {
        _registry.Add(sourceType, converter);
        return this;
    }

    /// <summary>
    /// Register a converter as resolvable by <see cref="Type"/>.
    /// </summary>
    /// <returns>The builder.</returns>
    public ConverterResolverBuilder Register<TSource, TConverter>()
    {
        Register(typeof(TSource), typeof(TConverter));
        return this;
    }

    /// <inheritdoc/>
    public ConverterResolver Build()
    {
        return new(_registry);
    }
}
