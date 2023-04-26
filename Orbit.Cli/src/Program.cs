using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Orbit.Core;
using Orbit.Core.Activities;
using StudioLE.CommandLine;

namespace Orbit.Cli;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder()
            .RegisterCustomLoggingProviders()
            .RegisterServices();
        CommandFactory factory = new(hostBuilder);
        RootCommand command = new CommandBuilder(factory)
            .Register<Create>()
            // .Register<Launch>()
            .Build();

        await command.InvokeAsync(args);
    }
}
