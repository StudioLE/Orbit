namespace StudioLE.CommandLine.Composition;

public class ObjectTree : ObjectTreeBase
{
    public object Instance { get; }

    public ObjectTree(object instance) : this(instance.GetType())
    {
        Instance = instance;
    }

    public ObjectTree(Type type)
    {
        Instance = Activator.CreateInstance(type) ?? throw new("Failed to create ObjectTree instance. Type does not have a parameterless constructor.");
        Type? underlyingType = Nullable.GetUnderlyingType(type);
        if(underlyingType is not null)
            type = underlyingType;
        SetProperties(type, this);
    }

    public static ObjectTree Create<T>()
    {
        return new(typeof(T));
    }
}
