namespace Orbit.Utils;

public static class StructHelpers
{
    public static T? FirstOrNull<T>(this IEnumerable<T> @this) where T : struct
    {
        return @this
            .Cast<T?>()
            .FirstOrDefault();
    }

    public static T? FirstOrNull<T>(this IEnumerable<T> @this, Func<T, bool> predicate) where T : struct
    {
        return @this
            .Where(predicate)
            .Cast<T?>()
            .FirstOrDefault();
    }
}
