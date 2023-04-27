using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudioLE.CommandLine.Composition;
using StudioLE.CommandLine.Utils;

namespace StudioLE.CommandLine;

public class CommandFactory
{
    private readonly IHostBuilder _hostBuilder;
    private readonly HashSet<Type> _parseableTypes = new()
    {
        typeof(string),
        typeof(int),
        typeof(double),
        typeof(Enum)
    };
    private readonly CommandOptionsFactory _optionsFactory;
    private readonly Dictionary<string, Option> _options = new();

    public Type? ActivityType { get; set; }

    public string? CommandName { get; set; }

    public string? Description { get; set; }

    public CommandFactory(IHostBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder;
        _optionsFactory = new(IsParseable);
    }

    public CommandFactory(IHostBuilder hostBuilder, HashSet<Type> parseableTypes)
    {
        _hostBuilder = hostBuilder;
        _parseableTypes = parseableTypes;
        _optionsFactory = new(IsParseable);
    }

    public Command Build()
    {
        if (ActivityType is null)
            throw new("Failed to build Command. ActivityType has not been set.");
        SetUnsetProperties();
        Command command = new(CommandName!, Description);
        MethodInfo activityMethod = ActivityType.GetMethod("Execute") ?? throw new("No Execute method");
        ObjectTree tree = CreateObjectTree(activityMethod);

        Option[] options = _optionsFactory.Create(tree);
        foreach (Option option in options)
        {
            _options.Add(option.Aliases.First(), option);
            command.AddOption(option);
        }
        Action<InvocationContext> handler = CreateHandler(ActivityType, activityMethod, tree);
        command.SetHandler(handler);
        return command;
    }

    private void SetUnsetProperties()
    {
        if (ActivityType is null)
            throw new("Failed to build Command. ActivityType has not been set.");
        CommandName ??= ActivityType?.Name.ToLower();
        DescriptionAttribute? descriptionAttribute = ActivityType?.GetAttribute<DescriptionAttribute>();
        Description ??= descriptionAttribute is null
            ? string.Empty
            : descriptionAttribute.Description;
    }

    private ObjectTree CreateObjectTree(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 1)
            throw new("Only a single parameter is permitted.");
        ParameterInfo parameter = parameters.First();
        return new(parameter.ParameterType);
    }

    private Action<InvocationContext> CreateHandler(Type activityType, MethodInfo activityMethod, ObjectTree tree)
    {
        return context =>
        {
            object parameter = GetOptionValue(context.BindingContext, tree);
            IHost host = _hostBuilder.Build();
            object activity = host.Services.GetRequiredService(activityType);
            activityMethod.Invoke(activity, new[] { parameter });
        };
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
            if (!IsParseable(factory.Type))
                continue;
            if (!_options.TryGetValue(factory.FullKey.ToLongOption(), out Option? option))
                continue;
            object? value = context.ParseResult.GetValueForOption(option);
            if (value is null)
                continue;
            if (!factory.Type.IsInstanceOfType(value))
                continue;
            factory.SetValue(value);
        }
        return tree.Instance;
    }
}
