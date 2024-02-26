namespace Orbit.Schema;

public record struct InstanceId : IEntityId<Instance>
{
    public const string Directory = "instances";

    /// <inheritdoc />
    public string Name { get; set; }

    public InstanceId(string name)
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
