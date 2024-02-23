

// ReSharper disable once CheckNamespace
namespace StudioLE.Serialization;

/// <summary>
/// Methods to help with <see cref="T:StudioLE.Serialization.IDeserializer"/>.
/// </summary>
public static class DeserializerHelpers
{
    /// <summary>
    /// Deserialize <paramref name="input"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="deserializer">The deserializer to use.</param>
    /// <param name="input">The reader to deserialize from.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value, or <see langword="null"/> if deserialization failed.</returns>
    public static T? Deserialize<T>(this IDeserializer deserializer, TextReader input) where T : struct
    {
        object? deserialized = deserializer.Deserialize(input, typeof(T));
        return deserialized is T value
            ? value
            : null;
    }

    /// <summary>
    /// Deserialize <paramref name="input"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="deserializer">The deserializer to use.</param>
    /// <param name="input">The string to deserialize.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value, or <see langword="null"/> if deserialization failed.</returns>
    public static T? Deserialize<T>(this IDeserializer deserializer, string input) where T : struct
    {
        using StringReader reader = new(input);
        return deserializer.Deserialize<T>(reader);
    }

    /// <summary>
    /// Deserialize the <paramref name="file"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="deserializer">The deserializer to use.</param>
    /// <param name="file">The serialized file.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value, or <see langword="null"/> if deserialization failed.</returns>
    public static T? Deserialize<T>(this IDeserializer deserializer, FileInfo file) where T : struct
    {
        using Stream stream = file.OpenRead();
        using StreamReader reader = new(stream);
        return deserializer.Deserialize<T>(reader);
    }
}
