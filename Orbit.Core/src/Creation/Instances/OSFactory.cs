using Orbit.Schema;
using Orbit.Utils;
using StudioLE.Patterns;

namespace Orbit.Creation.Instances;

/// <summary>
/// A factory for creating <see cref="OS"/> with default values.
/// </summary>
// ReSharper disable once InconsistentNaming
public class OSFactory : IFactory<OS, OS>
{
    private const string DefaultName = "ubuntu";
    private const string DefaultVersion = "jammy";

    /// <inheritdoc />
    public OS Create(OS source)
    {
        OS result = new()
        {
            Name = source.Name,
            Version = source.Version
        };
        if (!result.Name.IsNullOrEmpty() && !result.Version.IsNullOrEmpty())
            return result;

        result.Name = DefaultName;
        result.Version = DefaultVersion;

        return result;
    }
}
