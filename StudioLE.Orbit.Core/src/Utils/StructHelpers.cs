namespace StudioLE.Orbit.Utils;

/// <summary>
/// Methods to help with structs.
/// </summary>
public static class StructHelpers
{
    /// <summary>
    /// Get the first value in the sequence or <see langword="null"/> if the sequence is empty.
    /// </summary>
    /// <param name="this">The sequence.</param>
    /// <typeparam name="T">The sequence object type.</typeparam>
    /// <returns>
    /// The first value in the sequence or <see langword="null"/> if the sequence is empty.
    /// </returns>
    public static T? FirstOrNull<T>(this IEnumerable<T> @this) where T : struct
    {
        return @this
            .Cast<T?>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Get the first value in the sequence after filtering by <paramref name="predicate"/>
    /// or <see langword="null"/> if the sequence is empty.
    /// </summary>
    /// <param name="this">The sequence.</param>
    /// <param name="predicate">The predicate filter.</param>
    /// <typeparam name="T">The sequence object type.</typeparam>
    /// <returns>
    /// The first value in the sequence or <see langword="null"/> if the sequence is empty.
    /// </returns>
    public static T? FirstOrNull<T>(this IEnumerable<T> @this, Func<T, bool> predicate) where T : struct
    {
        return @this
            .Where(predicate)
            .Cast<T?>()
            .FirstOrDefault();
    }
}
