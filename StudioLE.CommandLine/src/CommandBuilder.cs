using System.CommandLine;
using StudioLE.CommandLine.Utils.Patterns;

namespace StudioLE.CommandLine;

public class CommandBuilder : IBuilder<RootCommand>
{
    private readonly CommandFactory _factory;
    private readonly List<Type> _activities = new();

    public CommandBuilder(CommandFactory factory)
    {
        _factory = factory;
    }

    public CommandBuilder Register(Type activity)
    {
        _activities.Add(activity);
        return this;
    }

    public CommandBuilder Register<TActivity>()
    {
        _activities.Add(typeof(TActivity));
        return this;
    }

    /// <inheritdoc />
    public RootCommand Build()
    {
        RootCommand root = new();
        Command[] commands = _activities
            .Select(_factory.Build)
            .ToArray();
        foreach (Command command in commands)
            root.AddCommand(command);
        return root;
    }
}
