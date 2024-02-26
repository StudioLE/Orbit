namespace Orbit.Schema;

public readonly record struct InstanceId : IEntityId<Instance>, IParsable<InstanceId>
{
    public const string Directory = "instances";
    private readonly string _name = string.Empty;

    public InstanceId(string name)
    {
        _name = name;
    }

    /// <inheritdoc/>
    public bool IsDefault()
    {
        return string.IsNullOrEmpty(_name);
    }

    /// <inheritdoc/>
    public string GetFilePath()
    {
        return Path.Combine(Directory, _name, _name + ".yml");
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _name;
    }

    /// <inheritdoc/>
    public static InstanceId Parse(string str, IFormatProvider? provider)
    {
        return new(str);
    }

    /// <inheritdoc/>
    public static bool TryParse(string? str, IFormatProvider? provider, out InstanceId result)
    {
        if (string.IsNullOrEmpty(str))
        {
            result = default;
            return false;
        }
        result = new(str);
        return true;
    }
}
