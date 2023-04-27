using System.CommandLine;
using Microsoft.Extensions.Hosting;
using StudioLE.CommandLine.Utils.Patterns;

namespace StudioLE.CommandLine;

public class CommandBuilder : IBuilder<RootCommand>
{
    private readonly IHostBuilder _hostBuilder;
    private readonly HashSet<Type> _parseableTypes =  new()
    {
        typeof(string),
        typeof(int),
        typeof(double),
        typeof(Enum)
    };
    private readonly List<CommandFactory> _factories = new();

    public CommandBuilder(IHostBuilder hostBuilder)
    {
        _hostBuilder = hostBuilder;
    }

    public CommandBuilder(IHostBuilder hostBuilder, HashSet<Type> parseableTypes)
    {
        _hostBuilder = hostBuilder;
        _parseableTypes = parseableTypes;
    }

    public CommandBuilder Register(Type activity)
    {
        CommandFactory factory = new(_hostBuilder, _parseableTypes)
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
