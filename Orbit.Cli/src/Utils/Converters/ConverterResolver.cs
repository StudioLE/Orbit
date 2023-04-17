using Orbit.Cli.Utils.Composition;
using Orbit.Core.Schema;
using StudioLE.Core.Results;

namespace Orbit.Cli.Utils.Converters;

/// <summary>
/// A dependency injection service to help resolve <see cref="ObjectTree"/> by <see cref="Type"/>.
/// </summary>
/// <remarks>
/// The resolver should be constructed via a <see cref="ConverterResolverBuilder"/>
/// and registered as a dependency injection service.
/// </remarks>
public class ConverterResolver
{
    private readonly IDictionary<Type, Type> _registry;

    /// <summary>
    /// Construct an <see cref="ConverterResolver"/> from an existing dictionary.
    /// </summary>
    /// <remarks>
    /// This constructor is used by <see cref="ConverterResolverBuilder"/>.
    /// </remarks>
    /// <param name="registry">The assemblies.</param>
    internal ConverterResolver(IDictionary<Type, Type> registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Resolve a <see cref="ObjectTree"/> by <see cref="Type"/>.
    /// </summary>
    /// <param name="key">The <see cref="ObjectTree"/> for <see cref="Type"/>.</param>
    public IResult<object> TryResolve(Type key)
    {
        if(!_registry.TryGetValue(key, out Type? type))
            return new Failure<object>("Key does not exist.");
        object? instance = Activator.CreateInstance(type);
        return instance is null
            ? new Failure<object>("Failed to construct instance.")
            : new Success<object>(instance);
    }

    public static ConverterResolver Default()
    {
        return new ConverterResolverBuilder()
            .Register<int, StringToInteger>()
            .Register<double, StringToDouble>()
            .Register<Platform, StringToEnum<Platform>>()
            .Build();
    }
}
