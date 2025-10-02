using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace StudioLE.Orbit.Utils.Serialization;

// TODO: Move to StudioLE.Serialization
/// <summary>
/// Convert <see cref="IParsable{TSelf}"/> to and from YAML.
/// </summary>
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

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        string value = parser.Consume<Scalar>().Value;
        return ParseableHelpers.InvokeParseMethodByReflection(type, value);
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        string serialized = value?.ToString() ?? string.Empty;
        Scalar scalar = new(serialized);
        emitter.Emit(scalar);
    }
}
