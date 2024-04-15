using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orbit.Utils.Networking;

/// <summary>
/// Convert an <see cref="IPv4"/> to and from JSON.
/// </summary>
// ReSharper disable once InconsistentNaming
public class IPv4JsonConverter : JsonConverter<IPv4>
{
    /// <inheritdoc />
    public override IPv4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string str = reader.GetString() ?? throw new JsonException($"Failed to convert {nameof(IPv4)}. Expected a string.");
        return new(str);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IPv4 value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
