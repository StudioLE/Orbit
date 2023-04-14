namespace Orbit.Core.Utils;

public static class AttributeHelpers
{
    public static Attribute? GetAttribute<T>(this object @this) where T : Attribute
    {
        return Attribute.GetCustomAttribute(@this.GetType(), typeof(T));
    }
}
