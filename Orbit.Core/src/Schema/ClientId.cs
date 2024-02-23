namespace Orbit.Schema;

public readonly record struct ClientId : IEntityId<Client>
{
    public const string Directory = "clients";

    /// <inheritdoc/>
    public string Name { get; init; }

    public ClientId(string name)
    {
        // TODO: Validate name
        Name = name;
    }

    /// <inheritdoc/>
    public string GetFilePath()
    {
        return Path.Combine(Directory, Name, Name + ".yml");
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name;
    }
}
