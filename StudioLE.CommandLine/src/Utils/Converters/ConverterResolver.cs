using System.Reflection;

namespace StudioLE.CommandLine.Utils.Converters;

/// <summary>
/// A dependency injection service to help resolve converts.
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
    /// Resolve a converter by <see cref="Type"/>.
    /// </summary>
    /// <param name="resultType">The converter for <see cref="Type"/>.</param>
    public bool CanResolve(Type resultType)
    {
        return _registry.ContainsKey(resultType);
    }

    /// <summary>
    /// Resolve a converter by <see cref="Type"/>.
    /// </summary>
    /// <param name="resultType">The converter for <see cref="Type"/>.</param>
    public Type? ResolveExact(Type resultType)
    {
        return _registry.TryGetValue(resultType, out Type? type)
            ? type
            : null;
    }

    /// <summary>
    /// Resolve a converter by <see cref="Type"/>.
    /// </summary>
    /// <param name="resultType">The converter for <see cref="Type"/>.</param>
    public Type? Resolve(Type resultType)
    {
        Type originalResultType = resultType;
        while (true)
        {
            if (_registry.TryGetValue(resultType, out Type? type))
                return type;
            if (resultType.BaseType is null)
                return null;
            resultType = resultType.BaseType;
        }
    }

    /// <summary>
    /// Resolve a converter by <see cref="Type"/>.
    /// </summary>
    /// <param name="resultType">The converter for <see cref="Type"/>.</param>
    public object? ResolveActivated(Type resultType)
    {
        Type? resolved = Resolve(resultType);
        return resolved is not null
            ? Activate(resultType, resolved)
            : null;
    }

    /// <summary>
    /// Resolve a converter by <see cref="Type"/>.
    /// </summary>
    public object? Activate(Type resultType, Type converterType)
    {
        ConstructorInfo[] constructors = converterType.GetConstructors();
        ConstructorInfo? parameterlessConstructor = constructors
            .FirstOrDefault(x => x.GetParameters().Length == 0);
        if(parameterlessConstructor is not null)
            return Activator.CreateInstance(converterType);
        ConstructorInfo? typeConstructor = constructors
            .FirstOrDefault(x => x.GetParameters().Length == 1
            && x.GetParameters().First().ParameterType == typeof(Type));
        if(typeConstructor is not null)
            return Activator.CreateInstance(converterType, resultType);
        return null;
    }

    public static ConverterResolver Default()
    {
        return new ConverterResolverBuilder()
            .Register<Enum, StringToEnum>()
            .Register<int, StringToInteger>()
            .Register<double, StringToDouble>()
            .Register<string, StringToString>()
            .Build();
    }
}
