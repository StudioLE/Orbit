using System.CommandLine;
using Cascade.Workflows;
using Microsoft.Extensions.DependencyInjection;
using StudioLE.Core.Patterns;

namespace StudioLE.CommandLine;

public class CommandBuilder : IBuilder<RootCommand>
{
    private readonly IServiceProvider _services;
    private readonly List<IActivity> _activities = new();

    public CommandBuilder(IServiceProvider services)
    {
        _services = services;
    }

    public CommandBuilder Register<TActivity>() where TActivity : IActivity
    {
        TActivity activity = _services.GetRequiredService<TActivity>();
        _activities.Add(activity);
        return this;
    }

    /// <inheritdoc/>
    public RootCommand Build()
    {
        RootCommand root = new();
        Command[] commands = _activities
            .Select(activity =>
            {
                CommandFactory factory = _services.GetRequiredService<CommandFactory>();
                return factory.Create(activity);
            })
            .ToArray();
        foreach (Command command in commands)
            root.AddCommand(command);
        return root;
    }
}
