using StudioLE.Core.Conversion;
using StudioLE.Core.Results;

namespace Orbit.Cli.Utils.Converters;

/// <inheritdoc />
public class TryStringToDouble : IConverter<string, IResult<double>>
{
    /// <inheritdoc />
    public IResult<double> Convert(string source)
    {
        return double.TryParse(source, out double value)
            ? new Success<double>(value)
            : new Failure<double>("Failed to convert string to integer.");
    }
}
