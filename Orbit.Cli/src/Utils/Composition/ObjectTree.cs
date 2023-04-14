namespace Orbit.Cli.Utils.Composition;

public class ObjectTree : ObjectTreeBase
{
    private readonly object _instance;
    public object Value => _instance;

    public ObjectTree(object instance)
    {
        _instance = instance;
        Type type = instance.GetType();
        Type? underlyingType = Nullable.GetUnderlyingType(type);
        if(underlyingType is not null)
            type = underlyingType;
        SetProperties(type, _instance);
    }

    public ObjectTree(Type type)
    {
        Type? underlyingType = Nullable.GetUnderlyingType(type);
        if(underlyingType is not null)
            type = underlyingType;
        _instance = Activator.CreateInstance(type) ?? throw new("Failed to activate instance.");
        SetProperties(type, _instance);
    }

    public static ObjectTree Create<T>()
    {
        return new(typeof(T));
    }
}
