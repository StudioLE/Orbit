using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Orbit.Core.Utils.Serialization.Yaml;

/// <summary>
/// A <see cref="INodeTypeResolver"/> that resolves <see cref="IReadOnlyCollection{T}"/>, <see cref="IReadOnlyList{T}"/>, and <see cref="IReadOnlyDictionary{TKey, TValue}"/> to <see cref="List{T}"/> and <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
/// <seealso href="https://github.com/aaubry/YamlDotNet/issues/236#issuecomment-632054372"/>
public sealed class ReadOnlyCollectionNodeTypeResolver : INodeTypeResolver
{
    private static readonly IReadOnlyDictionary<Type, Type> _customGenericInterfaceImplementations = new Dictionary<Type, Type>
    {
        { typeof(IReadOnlyCollection<>), typeof(List<>) },
        { typeof(IReadOnlyList<>), typeof(List<>) },
        { typeof(IReadOnlyDictionary<,>), typeof(Dictionary<,>) }
    };

    /// <inheritdoc/>
    public bool Resolve(NodeEvent? nodeEvent, ref Type type)
    {
        if (type.IsInterface
            && type.IsGenericType
            && _customGenericInterfaceImplementations.TryGetValue(type.GetGenericTypeDefinition(), out Type? concreteType))
        {
            type = concreteType.MakeGenericType(type.GetGenericArguments());
            return true;
        }
        return false;
    }
}
