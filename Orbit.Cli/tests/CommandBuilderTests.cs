using System.CommandLine;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Cli.Utils.CommandLine;
using Orbit.Core;
using Orbit.Core.Activities;

namespace Orbit.Cli.Tests;

internal sealed class CommandBuilderTests
{
    [Test]
    public void CommandBuilder_Build()
    {
        // Arrange
        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder()
            .RegisterCustomLoggingProviders()
            .RegisterServices();
        CommandFactory factory = new(hostBuilder);

        // Act
        RootCommand command = new CommandBuilder(factory)
            .Register<Create>()
            // .Register<Launch>()
            .Build();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(command.Options.Count, Is.EqualTo(0));
            Assert.That(command.Children.Count(), Is.EqualTo(1));
            Symbol createSymbol = command.Children.First();
            if (createSymbol is Command createCommand)
            {
                Assert.That(createCommand.Children.Count(), Is.EqualTo(16));
                Assert.That(createCommand.Options.Count, Is.EqualTo(16));
                Assert.That(createCommand.Arguments.Count, Is.EqualTo(0));
            }
            else
                Assert.Fail();
        });
    }
}
