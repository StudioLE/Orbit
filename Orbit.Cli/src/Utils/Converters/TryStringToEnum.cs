using StudioLE.Core.Conversion;
using StudioLE.Core.Results;

namespace Orbit.Cli.Utils.Converters;

/// <inheritdoc />
public class TryStringToEnum<TEnum> : IConverter<string, IResult<TEnum>> where TEnum : struct, Enum
{
    /// <inheritdoc />
    public IResult<TEnum> Convert(string source)
    {
        return Enum.TryParse(source, out TEnum value)
            ? new Success<TEnum>(value)
            : new Failure<TEnum>("Failed to convert string to integer.");
    }
}
