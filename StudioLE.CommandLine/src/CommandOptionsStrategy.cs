using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using StudioLE.CommandLine.Composition;
using StudioLE.CommandLine.Utils.Patterns;
using StudioLE.Core.System;

namespace StudioLE.CommandLine;

public class CommandOptionsStrategy : IStrategy<CommandFactory, IReadOnlyDictionary<string, Option>>
{
    private readonly HashSet<string> _optionAliases = new();
    private readonly IStrategy<Type, bool> _isParsableStrategy;

    public CommandOptionsStrategy(IStrategy<Type, bool> isParsableStrategy)
    {
        _isParsableStrategy = isParsableStrategy;
    }

    public IReadOnlyDictionary<string, Option> Execute(CommandFactory commandFactory)
    {
        if (commandFactory.Tree is null)
            throw new("Expected tree to be set.");
        return commandFactory
            .Tree
            .FlattenProperties()
            .Where(x => _isParsableStrategy.Execute(x.Type))
            .Select(CreateOptionForProperty)
            .ToDictionary(option => option.Aliases.First(), option => option);
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
}
