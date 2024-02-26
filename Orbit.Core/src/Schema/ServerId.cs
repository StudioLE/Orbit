namespace Orbit.Schema;

public readonly record struct ServerId : IEntityId<Server>, IParsable<ServerId>
{
    public const string Directory = "servers";
    private readonly string _name = string.Empty;

    public ServerId(string name)
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
    public static ServerId Parse(string str, IFormatProvider? provider)
    {
        return new(str);
    }

    /// <inheritdoc/>
    public static bool TryParse(string? str, IFormatProvider? provider, out ServerId result)
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
