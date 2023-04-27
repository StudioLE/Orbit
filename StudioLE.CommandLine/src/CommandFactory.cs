using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudioLE.CommandLine.Composition;
using StudioLE.CommandLine.Utils;
using StudioLE.Core.System;

namespace StudioLE.CommandLine;

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
    private readonly HashSet<string> _optionAliases = new();
    private readonly Dictionary<string, Option> _options = new();

    public Type? ActivityType { get; set; }

    public string? CommandName { get; set; }

    public string? Description { get; set; }

    public CommandFactory(IHostBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder;
    }

    public CommandFactory(IHostBuilder hostBuilder, HashSet<Type> parseableTypes)
    {
        _hostBuilder = hostBuilder;
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
        Option[] options = CreateOptionsForProperties(tree);
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

    private Action<InvocationContext> CreateHandler(Type activityType, MethodInfo activityMethod, ObjectTree tree)
    {
        return context =>
        {
            object parameter = GetOptionValue(context.BindingContext, tree);
            IHost host = _hostBuilder.Build();
            object activity = host.Services.GetRequiredService(activityType);
            activityMethod.Invoke(activity, new [] { parameter });
        };
    }

    private ObjectTree CreateObjectTree(MethodInfo method)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length != 1)
            throw new("Only a single parameter is permitted.");
        ParameterInfo parameter = parameters.First();
        return new(parameter.ParameterType);
    }

    private Option[] CreateOptionsForProperties(ObjectTree tree)
    {
         return tree
            .FlattenProperties()
            .Where(x => IsParseable(x.Type))
            .Select(CreateOptionForProperty)
            .ToArray();
    }

    private bool IsParseable(Type type)
    {
        return _parseableTypes.Contains(type)
               || _parseableTypes.Any(x => x.IsAssignableFrom(type));
    }

    private Option CreateOptionForProperty(ObjectTreeProperty tree)
    {
        IReadOnlyCollection<string> aliases = GetAliases(tree);
        Option option = CreateInstanceOfOption(tree);
        SetOptionValidator(tree, option);
        foreach (string alias in aliases)
            _optionAliases.Add(alias);
        return option;
    }

    private IReadOnlyCollection<string> GetAliases(ObjectTreeProperty tree)
    {
        HashSet<string> aliases = new()
        {
            tree.FullKey.ToLongOption()
        };
        if(!_optionAliases.Contains(tree.Key.ToLongOption()))
            aliases.Add(tree.Key.ToLongOption());
        if (tree.Parent is ObjectTreeProperty parent)
        {
            if(!_optionAliases.Contains(parent.FullKey.ToLongOption()))
                aliases.Add(parent.FullKey.ToLongOption());
            if(!_optionAliases.Contains(parent.Key.ToLongOption()))
                aliases.Add(parent.Key.ToLongOption());
        }
        return aliases;
    }

    private Option CreateInstanceOfOption(ObjectTreeProperty tree)
    {
        IReadOnlyCollection<string> aliases = GetAliases(tree);
        Type optionType = typeof(Option<>).MakeGenericType(tree.Type);
        object instance = Activator.CreateInstance(optionType, aliases.ToArray(), tree.HelperText) ?? throw new("Failed to construct option. Activator returned null.");
        if (instance is not Option option)
            throw new("Failed to construct option. Activator didn't return an Option.");
        return option;
    }

    private static void SetOptionValidator(ObjectTreeProperty tree, Option option)
    {
        ValidationAttribute[] validationAttributes = tree
            .Property
            .GetCustomAttributes<ValidationAttribute>()
            .ToArray();
        if (!validationAttributes.Any())
            return;
        option.AddValidator(result =>
        {
            object? value = result.GetValueForOption(option);
            List<ValidationResult> results = new();
            ValidationContext context = new(value!)
            {
                DisplayName = result.Token?.Value ?? throw new("Failed to get the value of the token.")
                // DisplayName = option.Description ?? throw new("Failed to get the value of the token.")
            };
            if (Validator.TryValidateValue(value!, context, results, validationAttributes))
                return;
            string message = results
                .Select(x => x.ErrorMessage)
                .OfType<string>()
                .Join();
            result.ErrorMessage = message;
        });
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
