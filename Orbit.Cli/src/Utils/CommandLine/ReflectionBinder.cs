using System.CommandLine;
using System.CommandLine.Binding;
using Orbit.Cli.Utils.Composition;
using Orbit.Cli.Utils.Converters;
using StudioLE.Core.Results;

namespace Orbit.Cli.Utils.CommandLine;

public class ReflectionBinder<T> : BinderBase<T>
{
    private readonly ConverterResolver _resolver;
    private readonly ObjectTree _tree;
    private readonly Dictionary<string, Option<string>> _options;

    public ReflectionBinder(ConverterResolver resolver, ObjectTree tree, Dictionary<string, Option<string>> options)
    {
        _resolver = resolver;
        _tree = tree;
        _options = options;
    }

    /// <inheritdoc />
    protected override T GetBoundValue(BindingContext bindingContext)
    {
        ObjectTreeProperty[] propertyFactories = _tree
            .FlattenProperties()
            .ToArray();
        foreach (ObjectTreeProperty factory in propertyFactories)
        {
            if(!_options.TryGetValue(factory.FullKey.ToLongOption(), out Option<string>? option))
                continue;
            string? stringValue = bindingContext.ParseResult.GetValueForOption(option);
            if (stringValue is null)
                continue;
            object value;
            if (factory.Type == typeof(string))
            {
                value = stringValue;
            }
            else
            {
                IResult<object> result = _resolver.TryResolve(factory.Type);
                if (result is not Success<object> success)
                    continue;
                dynamic converter = success.Value;
                value = converter.Convert(stringValue);
            }
            factory.SetValue(value);
        }
        return _tree.Value is T instanceValue
            ? instanceValue
            : throw new("Invalid type.");
    }
}
