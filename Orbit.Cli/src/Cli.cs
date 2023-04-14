using System.CommandLine;
using Orbit.Cli.Utils.CommandLine;
using Orbit.Cli.Utils.Converters;
using Orbit.Core;
using Orbit.Core.Schema;
using StudioLE.Core.Results;
using StudioLE.Core.System;

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
        CommandFactory<Instance> factory = new(_resolver)
        {
            Name = "create",
            Description = "Create something",
            Handler = CreateHandler
        };
        Command createCommand = factory.Build();

        RootCommand rootCommand = new("Sample app for System.CommandLine")
        {
            createCommand
        };
        rootCommand.SetHandler(() =>
        {
            Console.WriteLine("RootCommand called.");
        });
        await rootCommand.InvokeAsync(_args);
    }

    private void CreateHandler(Instance sourceInstance)
    {
        // IHostBuilder builder = Host.CreateDefaultBuilder(_args);
        // builder.RegisterCreateServices();
        // using IHost host = builder.Build();
        // ILogger<Create> logger = host.Services.GetRequiredService<ILogger<Create>>();
        IResult result = Create.Execute(sourceInstance);
        if(result is Failure failure)
        {
            Console.WriteLine("Failed to create an instance.");
            Console.WriteLine(failure.Errors.Join());
            return;
        }
        Console.WriteLine($"Created instance {sourceInstance.Id}");
    }
}
