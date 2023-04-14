using System.Collections.ObjectModel;

namespace Orbit.Cli.Utils.CommandLine;

public class Arguments : ReadOnlyCollection<string>
{
    /// <inheritdoc />
    public Arguments(IList<string> list) : base(list)
    {
    }
}
