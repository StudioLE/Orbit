using StudioLE.Core.Conversion;

namespace Orbit.Cli.Utils.Converters;

/// <inheritdoc />
public class StringToEnum<TEnum> : IConverter<string, TEnum> where TEnum : struct, Enum
{
    /// <inheritdoc />
    public TEnum Convert(string source)
    {
        return Enum.Parse<TEnum>(source);
    }
}
