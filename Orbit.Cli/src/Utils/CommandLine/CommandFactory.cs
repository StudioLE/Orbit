using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Cli.Utils.Composition;
using Orbit.Core.Utils;

namespace Orbit.Cli.Utils.CommandLine;

public class CommandFactory
{
    private readonly IHostBuilder _hostBuilder;
    private readonly HashSet<Type> _parseableTypes =  new()
        {
            typeof(string),
            typeof(int),
            typeof(double),
            typeof(Enum)
        };
    private readonly Dictionary<string, Option> _options = new();
    private readonly List<ObjectTree> _trees = new();

    public CommandFactory(IHostBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder;
    }

    public CommandFactory(IHostBuilder hostBuilder, HashSet<Type> parseableTypes)
    {
        _hostBuilder = hostBuilder;
        _parseableTypes = parseableTypes;
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
        Option[] options = CreateOptions(activityMethod);
        foreach (Option option in options)
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

    private Option[] CreateOptions(MethodInfo method)
    {
        return method
            .GetParameters()
            .SelectMany(CreateOptions)
            .ToArray();
    }

    private Option[] CreateOptions(ParameterInfo parameter)
    {
        ObjectTree tree = new(parameter.ParameterType);
        _trees.Add(tree);
        return CreateOptions(tree);
    }

    private Option[] CreateOptions(ObjectTree tree)
    {
        Option[] options = tree
            .FlattenProperties()
            .Select(CreateOption)
            .OfType<Option>()
            .ToArray();
        return options;
    }

    private Option? CreateOption(ObjectTreeProperty tree)
    {
        if (!IsParseable(tree.Type))
            return null;
        List<string> aliases = new()
        {
            tree.FullKey.ToLongOption()
        };
        if(!_options.ContainsKey(tree.Key.ToLongOption()))
            aliases.Add(tree.Key.ToLongOption());
        if (tree.Parent is ObjectTreeProperty parent)
        {
            if(!_options.ContainsKey(parent.FullKey.ToLongOption()))
                aliases.Add(parent.FullKey.ToLongOption());
            if(!_options.ContainsKey(parent.Key.ToLongOption()))
                aliases.Add(parent.Key.ToLongOption());
        }
        Type optionType = typeof(Option<>).MakeGenericType(tree.Type);
        object instance = Activator.CreateInstance(optionType, aliases.ToArray(), tree.HelperText) ?? throw new("Failed to construct option. Activator returned null.");
        if (instance is not Option option)
            throw new("Failed to construct option. Activator didn't return an Option.");
        foreach (string alias in aliases.Distinct())
            _options.Add(alias, option);
        return option;
    }

    private bool IsParseable(Type type)
    {
        return _parseableTypes.Contains(type)
               || _parseableTypes.Any(x => x.IsAssignableFrom(type));
    }

    private object GetOptionValue(BindingContext context, ObjectTree tree)
    {
        ObjectTreeProperty[] propertyFactories = tree
            .FlattenProperties()
            .ToArray();
        foreach (ObjectTreeProperty factory in propertyFactories)
        {
            if(!IsParseable(factory.Type))
                continue;
            if (!_options.TryGetValue(factory.FullKey.ToLongOption(), out Option? option))
                continue;
            object? value = context.ParseResult.GetValueForOption(option);
            if (value is null)
                continue;
            if(!factory.Type.IsInstanceOfType(value))
                continue;
            factory.SetValue(value);
        }
        return tree.Instance;
    }
}
