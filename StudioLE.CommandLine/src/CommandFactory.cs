using System.CommandLine;
using System.CommandLine.Invocation;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using StudioLE.CommandLine.Composition;
using StudioLE.CommandLine.Utils;

namespace StudioLE.CommandLine;

public class CommandFactory
{
    private readonly HashSet<Type> _parseableTypes = new()
    {
        typeof(string),
        typeof(int),
        typeof(double),
        typeof(Enum)
    };
    private readonly CommandOptionsFactory _optionsFactory;
    private readonly CommandHandlerFactory _handlerFactory;

    public Type? ActivityType { get; set; }

    public MethodInfo? ActivityMethod { get; private set; }

    public ObjectTree? Tree { get; private set; }

    public IReadOnlyDictionary<string, Option> Options { get; private set; } = new Dictionary<string, Option>();

    public CommandFactory(IHostBuilder hostBuilder)
    {
        _optionsFactory = new(IsParseable);
        _handlerFactory = new(hostBuilder, IsParseable);
    }

    public CommandFactory(IHostBuilder hostBuilder, HashSet<Type> parseableTypes) : this(hostBuilder)
    {
        _parseableTypes = parseableTypes;
    }

    public Command Build()
    {
        if (ActivityType is null)
            throw new("Failed to build Command. ActivityType has not been set.");
        ActivityMethod = ActivityType.GetMethod("Execute") ?? throw new("No Execute method");
        Tree = CreateObjectTree(ActivityMethod);
        Options = _optionsFactory.Create(this);
        string commandName = GetCommandName();
        string description = GetDescription();
        Command command = new(commandName, description);
        foreach ((string? _, Option? option) in Options)
            command.AddOption(option);
        Action<InvocationContext> handler = _handlerFactory.Create(this);
        command.SetHandler(handler);
        return command;
    }

    private string GetCommandName()
    {
        return ActivityType!.Name.ToLower();
    }

    private string GetDescription()
    {
        DescriptionAttribute? descriptionAttribute = ActivityType!.GetAttribute<DescriptionAttribute>();
        return descriptionAttribute is null
            ? string.Empty
            : descriptionAttribute.Description;
    }

    private static ObjectTree CreateObjectTree(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 1)
            throw new("Only a single parameter is permitted.");
        ParameterInfo parameter = parameters.First();
        return new(parameter.ParameterType);
    }

    private bool IsParseable(Type type)
    {
        return _parseableTypes.Contains(type)
               || _parseableTypes.Any(x => x.IsAssignableFrom(type));
    }
}
