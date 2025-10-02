namespace StudioLE.Orbit.Schema;

/// <summary>
/// The id of a <see cref="Client"/>.
/// </summary>
public readonly record struct ClientId : IEntityId<Client>, IParsable<ClientId>
{
    private readonly string _name = string.Empty;

    /// <summary>
    /// Create a new instance of <see cref="ClientId"/>.
    /// </summary>
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
