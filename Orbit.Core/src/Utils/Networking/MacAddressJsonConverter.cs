using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orbit.Utils.Networking;

/// <summary>
/// Convert an <see cref="MacAddress"/> to and from JSON.
/// </summary>
// ReSharper disable once InconsistentNaming
public class MacAddressJsonConverter : JsonConverter<MacAddress>
{
    /// <inheritdoc />
    public override MacAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string str = reader.GetString() ?? throw new JsonException($"Failed to convert {nameof(MacAddress)}. Expected a string.");
        return new(str);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MacAddress value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
