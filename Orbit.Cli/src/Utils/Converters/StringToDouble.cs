using StudioLE.Core.Conversion;

namespace Orbit.Cli.Utils.Converters;

/// <inheritdoc />
public class StringToDouble : IConverter<string, double>
{
    /// <inheritdoc />
    public double Convert(string source)
    {
        return double.Parse(source);
    }
}
