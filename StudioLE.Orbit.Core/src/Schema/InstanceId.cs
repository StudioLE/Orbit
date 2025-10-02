namespace StudioLE.Orbit.Schema;

/// <summary>
/// The id of an <see cref="Instance"/>.
/// </summary>
public readonly record struct InstanceId : IEntityId<Instance>, IParsable<InstanceId>
{
    private readonly string _name = string.Empty;

    /// <summary>
    /// Create a new instance of <see cref="InstanceId"/>.
    /// </summary>
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
