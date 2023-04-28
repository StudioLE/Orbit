using System.CommandLine;
using Microsoft.Extensions.Hosting;
using StudioLE.CommandLine.Utils.Patterns;

namespace StudioLE.CommandLine;

public class CommandBuilder : IBuilder<RootCommand>
{
    private readonly IHostBuilder _hostBuilder;
    private readonly IStrategy<Type, bool> _isParsableStrategy;
    private readonly List<CommandFactory> _factories = new();

    public CommandBuilder(IHostBuilder hostBuilder, IStrategy<Type, bool> isParsableStrategy)
    {
        _hostBuilder = hostBuilder;
        _isParsableStrategy = isParsableStrategy;
    }

    public CommandBuilder Register(Type activity)
    {
        CommandFactory factory = new(_hostBuilder, _isParsableStrategy)
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
