using System.Reflection;
using StudioLE.Core.Exceptions;

namespace StudioLE.CommandLine.Composition;

public class ObjectTreeProperty : ObjectTreeBase
{
    public PropertyInfo Property { get; }

    public ObjectTreeBase Parent { get; }

    public Type Type { get; private set; }

    public string Key { get; }

    public string FullKey { get; }

    public string HelperText { get; private set; }

    internal ObjectTreeProperty(PropertyInfo property, ObjectTreeBase parent, string? parentFullKey)
    {
        Property = property;
        Parent = parent;
        Type? underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
        Type type = underlyingType ?? property.PropertyType;
        Type = type;
        Key = property.Name;
        FullKey = parentFullKey is null
            ? Key
            : $"{parentFullKey}.{Key}";
        // TODO: Get the HelperText from DescriptionAttribute
        HelperText = property.Name;
        SetProperties(type, this);
    }

    private object GetParentInstance()
    {
        return Parent switch
        {
            ObjectTree tree => tree.Instance ?? throw new("Parent value isn't set."),
            ObjectTreeProperty parentProperty => parentProperty.GetValue(),
            _ => throw new TypeSwitchException<ObjectTreeBase>(Parent)
        };
    }

    /// <summary>
    /// Get the current value of the property.
    /// </summary>
    public object GetValue()
    {
        object parentInstance = GetParentInstance();
        return Property.GetValue(parentInstance) ?? throw new("Failed to get property value.");
    }

    /// <summary>
    /// Set the value of the property.
    /// </summary>
    /// <param name="value">The value.</param>
    public void SetValue(object value)
    {
        object parentInstance = GetParentInstance();
        Property.SetValue(parentInstance, value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return FullKey;
    }
}
