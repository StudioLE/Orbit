using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Orbit.Utils.Networking;

// ReSharper disable once InconsistentNaming
public class IPv4YamlConverter : IYamlTypeConverter
{
    /// <inheritdoc />
    public bool Accepts(Type type)
    {
        return type == typeof(IPv4);
    }

    /// <inheritdoc />
    public object ReadYaml(IParser parser, Type type)
    {
        string value = parser.Consume<Scalar>().Value;
        IPv4 ip = new(value);
        return ip;
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        string serialized = value?.ToString() ?? string.Empty;
        Scalar scalar = new(serialized);
        emitter.Emit(scalar);
    }
}
