using System.Reflection;

namespace Orbit.Utils.Serialization;

/// <summary>
/// Methods to help with <see cref="IParsable{TSelf}"/>.
/// </summary>
// TODO: Move to StudioLE.Serialization
internal static class ParseableHelpers
{
    /// <summary>
    /// Determine if <paramref name="type"/> has the <see cref="IParsable{TSelf}"/> interface.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    /// <see langword="true"/> if the type has the <see cref="IParsable{TSelf}"/> interface; otherwise, <see langword="false"/>.
    /// </returns>
    internal static bool HasParseableInterface(Type type)
    {
        return type
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>));
    }

    /// <summary>
    /// Invoke the Parse method on <paramref name="type"/> using reflection.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="serializedValue"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static object? InvokeParseMethodByReflection(Type type, string serializedValue)
    {
        MethodInfo? method = GetParseMethodByReflection(type);
        if (method is null)
            throw new InvalidOperationException("Failed to find Parse method.");
        object? parsed = method.Invoke(null, [serializedValue, null]);
        return parsed;
    }

    private static MethodInfo? GetParseMethodByReflection(Type type)
    {
        return type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, [typeof(string), typeof(IFormatProvider)]);
    }
}
