using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Orbit.Core;
using Orbit.Core.Activities;
using StudioLE.CommandLine;
using StudioLE.CommandLine.Utils.Patterns;
using DependencyInjectionHelper = Orbit.Core.DependencyInjectionHelper;

namespace Orbit.Cli;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        IHostBuilder hostBuilder = DependencyInjectionHelper.RegisterCustomLoggingProviders(Host
                .CreateDefaultBuilder())
            .RegisterServices();
        IStrategy<Type, bool> isParsableStrategy = new IsParseableStrategy();
        RootCommand command = new CommandBuilder(hostBuilder, isParsableStrategy)
            .Register<Create>()
            // .Register<Launch>()
            .Build();

        await command.InvokeAsync(args);
    }
}
