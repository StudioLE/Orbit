using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudioLE.CommandLine.Composition;

namespace StudioLE.CommandLine;

public class CommandHandlerFactory
{
    private readonly IHostBuilder _hostBuilder;
    private readonly Func<Type, bool> _isParseable;

    public CommandHandlerFactory(IHostBuilder hostBuilder, Func<Type, bool> isParseable)
    {
        _hostBuilder = hostBuilder;
        _isParseable = isParseable;
    }

    public Action<InvocationContext> Create(CommandFactory commandFactory)
    {
        if (commandFactory.Tree is null)
            throw new("Expected tree to be set.");
        if (commandFactory.ActivityType is null)
            throw new("Expected ActivityType to be set.");
        if (commandFactory.ActivityMethod is null)
            throw new("Expected ActivityMethod to be set.");
        return context =>
        {
            object parameter = GetOptionValue(context.BindingContext, commandFactory.Tree, commandFactory.Options);
            IHost host = _hostBuilder.Build();
            object activity = host.Services.GetRequiredService(commandFactory.ActivityType);
            commandFactory.ActivityMethod.Invoke(activity, new[] { parameter });
        };
    }

    private object GetOptionValue(BindingContext context, ObjectTree tree, IReadOnlyDictionary<string, Option> options)
    {
        ObjectTreeProperty[] propertyFactories = tree
            .FlattenProperties()
            .ToArray();
        foreach (ObjectTreeProperty factory in propertyFactories)
        {
            if (!_isParseable.Invoke(factory.Type))
                continue;
            if (!options.TryGetValue(factory.FullKey.ToLongOption(), out Option? option))
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
