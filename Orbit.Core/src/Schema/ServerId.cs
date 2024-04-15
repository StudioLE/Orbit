namespace Orbit.Schema;

/// <summary>
/// The id of a <see cref="Server"/>.
/// </summary>
public readonly record struct ServerId : IEntityId<Server>, IParsable<ServerId>
{
    private readonly string _name = string.Empty;

    /// <summary>
    /// Create a new instance of <see cref="ServerId"/>.
    /// </summary>
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
