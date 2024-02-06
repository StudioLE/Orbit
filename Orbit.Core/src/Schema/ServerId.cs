namespace Orbit.Schema;

public class ServerId : IEntityId<Server>
{
    public const string Directory = "servers";

    /// <inheritdoc />
    public string Name { get; }

    public ServerId(string name)
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
