using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Orbit.Utils.Networking;

/// <summary>
/// Convert an <see cref="MacAddress"/> to and from YAML.
/// </summary>
// ReSharper disable once InconsistentNaming
public class MacAddressYamlConverter : IYamlTypeConverter
{
    /// <inheritdoc />
    public bool Accepts(Type type)
    {
        return type == typeof(MacAddress);
    }

    /// <inheritdoc />
    public object ReadYaml(IParser parser, Type type)
    {
        string value = parser.Consume<Scalar>().Value;
        MacAddress macAddress = new(value);
        return macAddress;
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        string serialized = value?.ToString() ?? string.Empty;
        Scalar scalar = new(serialized);
        emitter.Emit(scalar);
    }
}
