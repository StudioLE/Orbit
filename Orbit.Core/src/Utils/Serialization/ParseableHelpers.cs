using System.Reflection;

namespace Orbit.Utils.Serialization;

internal static class ParseableHelpers
{
    internal static bool HasParseableInterface(Type type)
    {
        return type
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>));
    }

    internal static MethodInfo? GetParseMethodByReflection(Type type)
    {
        return type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, [typeof(string), typeof(IFormatProvider)]);
    }

    internal static object? InvokeParseMethodByReflection(Type type, string serializedValue)
    {
        MethodInfo? method = GetParseMethodByReflection(type);
        if (method is null)
            throw new InvalidOperationException("Failed to find Parse method.");
        object? parsed = method.Invoke(null, [serializedValue, null]);
        return parsed;
    }
}
