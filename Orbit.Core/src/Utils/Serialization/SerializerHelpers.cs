using StudioLE.Serialization;

namespace Orbit.Core.Utils.Serialization;

/// <summary>
/// Methods to help with <see cref="ISerializer"/>.
/// </summary>
public static class SerializerHelpers
{
    /// <summary>
    /// Serialize <paramref name="input"/> using <paramref name="serializer"/>.
    /// </summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="input">The object to serialize.</param>
    /// <returns>The serialized value.</returns>
    public static string Serialize(this ISerializer serializer, object input)
    {
        using StringWriter writer = new();
        serializer.Serialize(writer, input);
        return writer.ToString();
    }
}
