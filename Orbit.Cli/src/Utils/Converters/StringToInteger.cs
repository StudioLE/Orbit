using StudioLE.Core.Conversion;

namespace Orbit.Cli.Utils.Converters;

/// <inheritdoc />
public class StringToInteger : IConverter<string, int>
{
    /// <inheritdoc />
    public int Convert(string source)
    {
        return int.Parse(source);
    }
}
