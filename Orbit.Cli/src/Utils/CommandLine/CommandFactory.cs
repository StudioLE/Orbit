using System.CommandLine;
using System.CommandLine.Binding;
using Orbit.Cli.Utils.Composition;
using Orbit.Cli.Utils.Converters;

namespace Orbit.Cli.Utils.CommandLine;

public class CommandFactory<T> where T : new()
{
    private readonly ConverterResolver _resolver;
    private readonly Dictionary<string, Option<string>> _options = new();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Action<T> Handler { get; set; } = _ => { };

    public CommandFactory(ConverterResolver resolver)
    {
        _resolver = resolver;
    }

    public Command Build()
    {
        ObjectTree tree = ObjectTree.Create<T>();
        Option<string>[] options = CreateOptions(tree);
        ReflectionBinder<T> binder = new(_resolver, tree, _options);
        Command command = CreateCommand(options, binder);
        return command;
    }

    private Option<string>[] CreateOptions(ObjectTree tree)
    {
        Option<string>[] options = tree
            .FlattenProperties()
            .Select(CreateOption)
            .ToArray();
        return options;
    }

    private Option<string> CreateOption(ObjectTreeProperty tree)
    {
        string[] aliases = _options.ContainsKey(tree.Key.ToLongOption())
            ? new[] { tree.FullKey.ToLongOption() }
            : new[] { tree.FullKey.ToLongOption(), tree.Key.ToLongOption() };
        Option<string> option = new(aliases, tree.HelperText);
        foreach (string alias in aliases.Distinct())
            _options.Add(alias, option);
        return option;
    }

    private Command CreateCommand(Option<string>[] options, BinderBase<T> binder)
    {
        Command command = new(Name, Description);
        foreach (Option<string> option in options)
            command.AddOption(option);
        command.SetHandler(Handler, binder);
        return command;
    }
}
