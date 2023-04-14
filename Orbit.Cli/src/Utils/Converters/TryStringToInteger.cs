using StudioLE.Core.Conversion;
using StudioLE.Core.Results;

namespace Orbit.Cli.Utils.Converters;

/// <inheritdoc />
public class TryStringToInteger : IConverter<string, IResult<int>>
{
    /// <inheritdoc />
    public IResult<int> Convert(string source)
    {
        return int.TryParse(source, out int value)
            ? new Success<int>(value)
            : new Failure<int>("Failed to convert string to integer.");
    }
}
