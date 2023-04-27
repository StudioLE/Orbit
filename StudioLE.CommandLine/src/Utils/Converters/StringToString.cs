using StudioLE.Core.Conversion;

namespace StudioLE.CommandLine.Utils.Converters;

/// <inheritdoc />
public class StringToString : IConverter<string, string?>
{
    /// <inheritdoc />
    public string Convert(string source)
    {
        return source;
    }
}
