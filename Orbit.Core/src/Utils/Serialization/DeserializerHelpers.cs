using Microsoft.Extensions.FileProviders;

namespace Orbit.Core.Utils.Serialization;

/// <summary>
/// Methods to help with <see cref="IDeserializer"/>.
/// </summary>
public static class DeserializerHelpers
{
    /// <summary>
    /// Deserialize the <paramref name="file"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="deserializer">The deserializer to use.</param>
    /// <param name="file">The serialized file.</param>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized value.</returns>
    public static T Deserialize<T>(this IDeserializer deserializer, IFileInfo file)
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
    public static T Deserialize<T>(this IDeserializer deserializer, FileInfo file)
    {
        using Stream stream = file.OpenRead();
        using StreamReader reader = new(stream);
        return deserializer.Deserialize<T>(reader);
    }
}
