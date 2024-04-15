namespace Orbit.Utils;

/// <summary>
/// Methods to help with default values.
/// </summary>
public static class DefaultHelpers
{
    /// <summary>
    /// Is this a default value?
    /// </summary>
    public static bool IsDefault(this string @this)
    {
        return string.IsNullOrEmpty(@this);
    }

    /// <summary>
    /// Is this a default value?
    /// </summary>
    public static bool IsDefault(this int @this)
    {
        return @this == default;
    }

    /// <summary>
    /// Is this a default value?
    /// </summary>
    public static bool IsDefault<T>(this IReadOnlyCollection<T> @this)
    {
        return @this.Count == 0;
    }

    /// <summary>
    /// Is this a default value?
    /// </summary>
    public static bool IsDefault<T>(this T @this) where T : Enum
    {
        bool isDefault = @this.Equals(default(T));
        return isDefault;
    }
}
