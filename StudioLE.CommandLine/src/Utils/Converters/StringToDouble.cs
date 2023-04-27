using StudioLE.Core.Conversion;

namespace StudioLE.CommandLine.Utils.Converters;

/// <inheritdoc />
public class StringToDouble : IConverter<string, double?>
{
    /// <inheritdoc />
    public double? Convert(string source)
    {
        return double.TryParse(source, out double result)
            ? result
            : null;
    }
}
