namespace Orbit.Utils;

public static class DefaultHelpers
{
    public static bool IsDefault(this string @this)
    {
        return string.IsNullOrEmpty(@this);
    }

    public static bool IsDefault(this int @this)
    {
        return @this == default;
    }

    public static bool IsDefault<T>(this IReadOnlyCollection<T> @this)
    {
        return @this.Count == 0;
    }

    public static bool IsDefault<T>(this T @this) where T : Enum
    {
        bool isDefault = @this.Equals(default(T));
        return isDefault;
    }
}
