using Orbit.Core.Utils;

namespace Orbit.Core.Schema;

// ReSharper disable once InconsistentNaming
public sealed class OS
{
    private const string DefaultName = "ubuntu";
    private const string DefaultVersion = "jammy";

    public string Name { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public void Review()
    {
        if (!Name.IsNullOrEmpty() && !Version.IsNullOrEmpty())
            return;
        
        Name = DefaultName;
        Version = DefaultVersion;
    }
}
