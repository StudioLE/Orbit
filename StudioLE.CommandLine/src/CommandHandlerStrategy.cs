using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Cascade.Workflows;
using StudioLE.CommandLine.Composition;
using StudioLE.Core.Patterns;

namespace StudioLE.CommandLine;

public interface ICommandHandlerStrategy : IStrategy<CommandFactory, Action<InvocationContext>>
{
}

public class CommandHandlerStrategy : ICommandHandlerStrategy
{
    private readonly IIsParseableStrategy _isParsableStrategy;

    public CommandHandlerStrategy(IIsParseableStrategy isParsableStrategy)
    {
        _isParsableStrategy = isParsableStrategy;
    }

    public Action<InvocationContext> Execute(CommandFactory commandFactory)
    {
        if (commandFactory.InputTree is null)
            throw new("Expected input tree to be set.");
        return context =>
        {
            SetInputTreeValueFromOptions(context.BindingContext, commandFactory);
            object input = commandFactory.InputTree.Instance;
            commandFactory.Activity.Execute(input);
        };
    }

    private void SetInputTreeValueFromOptions(BindingContext context, CommandFactory commandFactory)
    {
        ObjectTreeProperty[] propertyFactories = commandFactory
            .InputTree!
            .FlattenProperties()
            .ToArray();
        foreach (ObjectTreeProperty factory in propertyFactories)
        {
            if (!_isParsableStrategy.Execute(factory.Type))
                continue;
            if (!commandFactory.Options.TryGetValue(factory.FullKey.ToLongOption(), out Option? option))
                continue;
            object? value = context.ParseResult.GetValueForOption(option);
            if (value is null)
                continue;
            if (!factory.Type.IsInstanceOfType(value))
                continue;
            factory.SetValue(value);
        }
    }
}
