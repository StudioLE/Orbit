using System.CommandLine;
using System.CommandLine.Invocation;
using Cascade.Workflows;
using StudioLE.CommandLine.Composition;
using StudioLE.Core.Patterns;

namespace StudioLE.CommandLine;

public class CommandFactory : IFactory<IActivity, Command>
{
    private readonly ICommandOptionsStrategy _optionsStrategy;
    private readonly ICommandHandlerStrategy _handlerStrategy;

    public IActivity Activity { get; private set; } = null!;

    public ObjectTree? InputTree { get; private set; }

    public IReadOnlyDictionary<string, Option> Options { get; private set; } = new Dictionary<string, Option>();

    public CommandFactory(ICommandOptionsStrategy optionsStrategy, ICommandHandlerStrategy handlerStrategy)
    {
        _optionsStrategy = optionsStrategy;
        _handlerStrategy = handlerStrategy;
    }

    public Command Create(IActivity activity)
    {
        Activity = activity;
        InputTree = CreateObjectTree();
        Options = _optionsStrategy.Execute(this);
        string commandName = GetCommandName();
        string description = GetDescription();
        Command command = new(commandName, description);
        foreach ((string? _, Option? option) in Options)
            command.AddOption(option);
        Action<InvocationContext> handler = _handlerStrategy.Execute(this);
        command.SetHandler(handler);
        return command;
    }

    private ObjectTree CreateObjectTree()
    {
        Type inputType = Activity.GetInputType();
        return new(inputType);
    }

    private string GetCommandName()
    {
        return Activity.GetName().ToLower();
    }

    private string GetDescription()
    {
        return Activity.GetDescription();
    }
}
