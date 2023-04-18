using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Cli.Utils.Composition;
using Orbit.Cli.Utils.Converters;
using Orbit.Core.Utils;
using StudioLE.Core.Results;

namespace Orbit.Cli.Utils.CommandLine;

public class CommandFactory
{
    private readonly IHostBuilder _hostBuilder;
    private readonly ConverterResolver _resolver;
    private readonly Dictionary<string, Option<string>> _options = new();
    private readonly List<ObjectTree> _trees = new();

    public CommandFactory(IHostBuilder hostBuilder, ConverterResolver resolver)
    {
        _hostBuilder = hostBuilder;
        _resolver = resolver;
    }

    public Command Build(Type activityType)
    {
        _options.Clear();
        _trees.Clear();
        string name = activityType.Name.ToLower();
        DescriptionAttribute? descriptionAttribute = activityType.GetAttribute<DescriptionAttribute>();
        string description = descriptionAttribute is null
            ? string.Empty
            : descriptionAttribute.Description;
        Command command = new(name, description);
        MethodInfo activityMethod = activityType.GetMethod("Execute") ?? throw new("No Execute method");
        Option<string>[] options = CreateOptions(activityMethod);
        foreach (Option<string> option in options)
            command.AddOption(option);
        Action<InvocationContext> handler = context =>
        {
            object[] parameters = _trees
                .Select(tree => GetOptionValue(context.BindingContext, tree))
                .ToArray();
            Console.WriteLine("Hello, world");
            IHost host = _hostBuilder.Build();
            object activity = host.Services.GetRequiredService(activityType);
            activityMethod.Invoke(activity, parameters);
        };
        command.SetHandler(handler);
        return command;
    }

    private Option<string>[] CreateOptions(MethodInfo method)
    {
        return method
            .GetParameters()
            .SelectMany(CreateOptions)
            .ToArray();
    }

    private Option<string>[] CreateOptions(ParameterInfo parameter)
    {
        ObjectTree tree = new(parameter.ParameterType);
        _trees.Add(tree);
        return CreateOptions(tree);
    }

    private Option<string>[] CreateOptions(ObjectTree tree)
    {
        Option<string>[] options = tree
            .FlattenProperties()
            .Select(CreateOption)
            .ToArray();
        return options;
    }

    private Option<string> CreateOption(ObjectTreeProperty tree)
    {
        string[] aliases = _options.ContainsKey(tree.Key.ToLongOption())
            ? new[] { tree.FullKey.ToLongOption() }
            : new[] { tree.FullKey.ToLongOption(), tree.Key.ToLongOption() };
        Option<string> option = new(aliases, tree.HelperText);
        foreach (string alias in aliases.Distinct())
            _options.Add(alias, option);
        return option;
    }

    private object GetOptionValue(BindingContext context, ObjectTree tree)
    {
        ObjectTreeProperty[] propertyFactories = tree
            .FlattenProperties()
            .ToArray();
        foreach (ObjectTreeProperty factory in propertyFactories)
        {
            if(!_options.TryGetValue(factory.FullKey.ToLongOption(), out Option<string>? option))
                continue;
            string? stringValue = context.ParseResult.GetValueForOption(option);
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
        return tree.Instance;
    }
}
