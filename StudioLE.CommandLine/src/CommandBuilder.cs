using System.CommandLine;
using Microsoft.Extensions.Hosting;
using StudioLE.Core.Patterns;

namespace StudioLE.CommandLine;

public class CommandBuilder : IBuilder<RootCommand>
{
    private readonly List<CommandFactory> _factories = new();

    private readonly IHostBuilder _activityHostBuilder;
    private readonly IIsParseableStrategy _isParsableStrategy;

    public CommandBuilder(IHostBuilder activityHostBuilder, IIsParseableStrategy isParsableStrategy)
    {
        _activityHostBuilder = activityHostBuilder;
        _isParsableStrategy = isParsableStrategy;
    }

    public CommandBuilder Register(Type activity)
    {
        // TODO: This is manual DI
        ICommandOptionsStrategy optionsStrategy = new CommandOptionsStrategy(_isParsableStrategy);
        ICommandHandlerStrategy handlerStrategy = new CommandHandlerStrategy(_activityHostBuilder, _isParsableStrategy);
        CommandFactory factory = new(optionsStrategy, handlerStrategy)
        {
            ActivityType = activity
        };
        _factories.Add(factory);
        return this;
    }

    public CommandBuilder Register<TActivity>()
    {
        Register(typeof(TActivity));
        return this;
    }

    /// <inheritdoc />
    public RootCommand Build()
    {
        RootCommand root = new();
        Command[] commands = _factories
            .Select(factory => factory.Build())
            .ToArray();
        foreach (Command command in commands)
            root.AddCommand(command);
        return root;
    }
}
