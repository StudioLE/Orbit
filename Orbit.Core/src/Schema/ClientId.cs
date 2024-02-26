namespace Orbit.Schema;

public readonly record struct ClientId : IEntityId<Client>, IParsable<ClientId>
{
    public const string Directory = "clients";
    private readonly string _name = string.Empty;

    public ClientId(string name)
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
    public static ClientId Parse(string str, IFormatProvider? provider)
    {
        return new(str);
    }

    /// <inheritdoc/>
    public static bool TryParse(string? str, IFormatProvider? provider, out ClientId result)
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
