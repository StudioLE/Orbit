using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orbit.Utils.Serialization;

// ReSharper disable once InconsistentNaming
public class ParseableJsonConverter : JsonConverter<object>
{
    /// <inheritdoc/>
    public override bool CanConvert(Type type)
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
    public override object? Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        string value = reader.GetString() ?? throw new JsonException("Failed to read. Expected a string.");
        return ParseableHelpers.InvokeParseMethodByReflection(type, value);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        string serialized = value.ToString() ?? string.Empty;
        writer.WriteStringValue(serialized);
    }
}
