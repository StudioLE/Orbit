using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orbit.Utils.Networking;

// ReSharper disable once InconsistentNaming
public class IPv6JsonConverter : JsonConverter<IPv6>
{
    /// <inheritdoc />
    public override IPv6 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string str = reader.GetString() ?? throw new JsonException($"Failed to convert {nameof(IPv6)}. Expected a string.");
        return new(str);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IPv6 value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
