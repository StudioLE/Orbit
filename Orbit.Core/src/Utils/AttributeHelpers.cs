namespace Orbit.Core.Utils;

public static class AttributeHelpers
{
    public static T? GetAttribute<T>(this object @this) where T : Attribute
    {
        return Attribute.GetCustomAttribute(@this.GetType(), typeof(T)) as T;
    }

    public static T? GetAttribute<T>(this Type @this) where T : Attribute
    {
        return Attribute.GetCustomAttribute(@this, typeof(T)) as T;
    }
}
