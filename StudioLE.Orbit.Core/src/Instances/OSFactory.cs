using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Utils;
using StudioLE.Patterns;

namespace StudioLE.Orbit.Instances;

/// <summary>
/// A factory for creating <see cref="OS"/> with default values.
/// </summary>
// ReSharper disable once InconsistentNaming
public class OSFactory : IFactory<OS, OS>
{
    private const string DefaultName = "ubuntu";
    private const string DefaultVersion = "jammy";

    /// <inheritdoc />
    public OS Create(OS os)
    {
        bool isSet = !os.Name.IsDefault() && !os.Version.IsDefault();
        return isSet
            ? os
            : new()
            {
                Name = DefaultName,
                Version = DefaultVersion
            };
    }
}
