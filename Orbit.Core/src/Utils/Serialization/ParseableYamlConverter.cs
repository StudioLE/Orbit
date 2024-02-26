using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Orbit.Utils.Serialization;

public class ParseableYamlConverter : IYamlTypeConverter
{
    /// <inheritdoc/>
    public bool Accepts(Type type)
    {
        if (type.Namespace is null)
            return false;
        if (type.Namespace.StartsWith("System"))
            return false;
        if (type.Namespace.StartsWith("Microsoft"))
            return false;
        return ParseableHelpers.HasParseableInterface(type);
    }

    /// <inheritdoc/>
    public object? ReadYaml(IParser parser, Type type)
    {
        string value = parser.Consume<Scalar>().Value;
        return ParseableHelpers.InvokeParseMethodByReflection(type, value);
    }

    /// <inheritdoc/>
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        string serialized = value?.ToString() ?? string.Empty;
        Scalar scalar = new(serialized);
        emitter.Emit(scalar);
    }
}
