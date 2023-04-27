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

    public string? CommandName { get; set; }

    public string? Description { get; set; }

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
        SetUnsetProperties();
        Command command = new(CommandName!, Description);
        MethodInfo activityMethod = ActivityType.GetMethod("Execute") ?? throw new("No Execute method");
        ObjectTree tree = CreateObjectTree(activityMethod);

        Option[] options = _optionsFactory.Create(tree);
        Dictionary<string, Option> optionsDictionary = new();
        foreach (Option option in options)
        {
            optionsDictionary.Add(option.Aliases.First(), option);
            command.AddOption(option);
        }
        Action<InvocationContext> handler = _handlerFactory.Create(ActivityType, activityMethod, tree, optionsDictionary);
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
