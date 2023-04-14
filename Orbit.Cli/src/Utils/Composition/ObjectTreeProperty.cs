using System.ComponentModel.DataAnnotations;
using System.Reflection;
using StudioLE.Core.Results;

namespace Orbit.Cli.Utils.Composition;

public class ObjectTreeProperty : ObjectTreeBase
{
    private readonly object _parentInstance;
    private readonly PropertyInfo _property;

    public Type Type { get; private set; }

    public string Key { get; private set; }

    public string FullKey { get; private set; }

    public string HelperText { get; private set; }

    internal ObjectTreeProperty(PropertyInfo property, object parentInstance, string? parentFullKey)
    {
        _property = property;
        _parentInstance = parentInstance;
        Type? underlyingType = Nullable.GetUnderlyingType(property.PropertyType);
        Type type = underlyingType ?? property.PropertyType;
        Type = type;
        Key = property.Name;
        FullKey = parentFullKey is null
            ? Key
            : $"{parentFullKey}.{Key}";
        // TODO: Get the HelperText from DescriptionAttribute
        HelperText = property.Name;
        object instance = GetValue();
        SetProperties(type, instance);
    }

    /// <summary>
    /// Get the current value of the property.
    /// </summary>
    public object GetValue()
    {
        return _property.GetValue(_parentInstance) ?? throw new("Failed to get property value.");
    }

    /// <summary>
    /// Set the value of the property.
    /// </summary>
    /// <param name="value">The value.</param>
    public void SetValue(object value)
    {
        _property.SetValue(_parentInstance, value);
    }

    /// <summary>
    /// Validate the value of the property according to the <see cref="System.ComponentModel.DataAnnotations"/>
    /// using <see cref="Validator.TryValidateProperty"/>
    /// </summary>
    /// <returns><see cref="Success"/> if valid otherwise <see cref="Failure"/> with errors messages stored to <see cref="Failure.Errors"/>.</returns>
    public IResult Validate()
    {
        ValidationContext context = new(_parentInstance);
        List<ValidationResult> results = new();
        return Validator.TryValidateObject(_parentInstance, context, results)
            ? new Success()
            : new Failure(results
                .Select(x => x.ErrorMessage)
                .OfType<string>()
                .ToArray());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return FullKey;
    }
}
