using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudioLE.CommandLine.Composition;
using StudioLE.CommandLine.Utils.Patterns;

namespace StudioLE.CommandLine;

public interface ICommandHandlerStrategy : IStrategy<CommandFactory, Action<InvocationContext>>
{
}

public class CommandHandlerStrategy : ICommandHandlerStrategy
{
    private readonly IHostBuilder _hostBuilder;
    private readonly IIsParseableStrategy _isParsableStrategy;

    public CommandHandlerStrategy(IHostBuilder hostBuilder, IIsParseableStrategy isParsableStrategy)
    {
        _hostBuilder = hostBuilder;
        _isParsableStrategy = isParsableStrategy;
    }

    public Action<InvocationContext> Execute(CommandFactory commandFactory)
    {
        if (commandFactory.Tree is null)
            throw new("Expected tree to be set.");
        if (commandFactory.ActivityType is null)
            throw new("Expected ActivityType to be set.");
        if (commandFactory.ActivityMethod is null)
            throw new("Expected ActivityMethod to be set.");
        return context =>
        {
            object parameter = GetOptionValue(context.BindingContext, commandFactory);
            IHost host = _hostBuilder.Build();
            object activity = host.Services.GetRequiredService(commandFactory.ActivityType);
            commandFactory.ActivityMethod.Invoke(activity, new[] { parameter });
        };
    }

    private object GetOptionValue(BindingContext context, CommandFactory commandFactory)
    {
        ObjectTreeProperty[] propertyFactories = commandFactory
            .Tree!
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
        return commandFactory.Tree.Instance;
    }
}
