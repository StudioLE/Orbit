namespace Orbit.Core.Schema;

public class NetworkId : IEntityId<Network>
{
    public const string Directory = "networks";

    /// <inheritdoc />
    public string Name { get; }

    public NetworkId(string name)
    {
        // TODO: Validate name
        Name = name;
    }

    /// <inheritdoc/>
    public string GetFilePath()
    {
        return Path.Combine(Directory, Name, Name + ".yml");
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Name;
    }
}
