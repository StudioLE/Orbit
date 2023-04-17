using System.CommandLine;
using Orbit.Cli.Utils.CommandLine;
using Orbit.Cli.Utils.Converters;
using Orbit.Core.Activities;
using Orbit.Core.Schema;

namespace Orbit.Cli;

public class Cli
{
    private readonly ConverterResolver _resolver;
    private string[] _args = Array.Empty<string>();

    public Cli()
    {
        _resolver = ConverterResolver.Default();
    }

    public Cli(ConverterResolver resolver)
    {
        _resolver = resolver;
    }

    public async Task Run(params string[] args)
    {
        _args = args;

        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        CommandFactory<Instance> factory = new(_resolver)
        {
            Name = "create",
            Description = "Create something",
            Handler = CreateHandler
        };
        Command createCommand = factory.Build();

        Command launchCommand = new("launch", "Launch an instance");
        launchCommand.SetHandler(LaunchHandler);


        RootCommand rootCommand = new("Sample app for System.CommandLine")
        {
            createCommand,
            launchCommand
        };
        rootCommand.SetHandler(() =>
        {
            Console.WriteLine("RootCommand called.");
        });
        await rootCommand.InvokeAsync(_args);
    }

    private void CreateHandler(Instance sourceInstance)
    {
        Create create = new();
        create.Execute(sourceInstance);
    }

    private void LaunchHandler()
    {
        Launch launch = new();
        launch.Execute("test");
    }
}
