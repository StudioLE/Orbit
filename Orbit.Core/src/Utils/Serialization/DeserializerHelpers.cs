using Microsoft.Extensions.FileProviders;
using StudioLE.Serialization;

namespace Orbit.Core.Utils.Serialization;

/// <summary>
/// Methods to help with <see cref="IDeserializer"/>.
/// </summary>
public static class DeserializerHelpers
{
    /// <summary>
    /// Deserialize <paramref name="input"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="deserializer">The deserializer to use.</param>
    /// <param name="input">The string to deserialize.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value.</returns>
    public static T? Deserialize<T>(this IDeserializer deserializer, TextReader input) where T : class
    {
        object? obj = deserializer.Deserialize(input, typeof(T));
        return obj as T;
    }

    /// <summary>
    /// Deserialize <paramref name="input"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="deserializer">The deserializer to use.</param>
    /// <param name="input">The string to deserialize.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value.</returns>
    public static T? Deserialize<T>(this IDeserializer deserializer, string input) where T : class
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
    /// <returns>The deserialized value.</returns>
    public static T? Deserialize<T>(this IDeserializer deserializer, IFileInfo file) where T : class
    {
        using Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return deserializer.Deserialize<T>(reader);
    }

    /// <summary>
    /// Deserialize the <paramref name="file"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="deserializer">The deserializer to use.</param>
    /// <param name="file">The serialized file.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value.</returns>
    public static T? Deserialize<T>(this IDeserializer deserializer, FileInfo file) where T : class
    {
        using Stream stream = file.OpenRead();
        using StreamReader reader = new(stream);
        return deserializer.Deserialize<T>(reader);
    }
}
