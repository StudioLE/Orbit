namespace Orbit.Core.Utils.Serialization;

/// <summary>
/// A generic object serializer.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// The file extension of the serialized format.
    /// </summary>
    public string FileExtension { get; }

    /// <summary>
    /// Serialize the <paramref name="obj"/> to the <paramref name="writer"/>.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    /// <param name="obj">The object to serialize.</param>
    public void Serialize(TextWriter writer, object? obj);
}
