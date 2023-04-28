using StudioLE.CommandLine.Utils.Patterns;

namespace StudioLE.CommandLine;

public class IsParseableStrategy : IStrategy<Type, bool>
{
    private readonly HashSet<Type> _parseableTypes = new()
    {
        typeof(string),
        typeof(int),
        typeof(double),
        typeof(Enum)
    };

    public IsParseableStrategy()
    {
    }

    public IsParseableStrategy(HashSet<Type> parseableTypes)
    {
        _parseableTypes = parseableTypes;
    }

    /// <inheritdoc />
    public bool Execute(Type type)
    {
        return _parseableTypes.Contains(type)
               || _parseableTypes.Any(x => x.IsAssignableFrom(type));
    }
}
