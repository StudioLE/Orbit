using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Cli.Utils.CommandLine;
using Orbit.Cli.Utils.Converters;
using Orbit.Core;
using Orbit.Core.Activities;
using Orbit.Core.Schema;

namespace Orbit.Cli;

public class Cli
{
    private readonly string[] _args;
    private readonly ConverterResolver _resolver;

    public Cli(string[] args, ConverterResolver resolver)
    {
        _args = args;
        _resolver = resolver;
    }


    public async Task Run()
    {

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
        using IHost host = Host
            .CreateDefaultBuilder(_args)
            .RegisterCreateServices()
            .Build();
        Create create = host.Services.GetRequiredService<Create>();
        Instance? createdInstance = create.Execute(sourceInstance);
        if(createdInstance is not null)
            Console.WriteLine($"Created instance {createdInstance.Id}");
    }

    private void LaunchHandler()
    {
        using IHost host = Host
            .CreateDefaultBuilder(_args)
            .RegisterLaunchServices()
            .Build();
        Launch launch = host.Services.GetRequiredService<Launch>();
        launch.Execute("blah");
        // if(createdInstance is not null)
        //     Console.WriteLine($"Created instance {createdInstance.Id}");
    }
}
