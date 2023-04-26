using System.CommandLine;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using StudioLE.CommandLine.Tests.Resources;

namespace StudioLE.CommandLine.Tests;

internal sealed class CommandBuilderTests
{
    [Test]
    public void CommandBuilder_Build()
    {
        // Arrange
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
        CommandFactory factory = new(hostBuilder);

        // Act
        RootCommand command = new CommandBuilder(factory)
            .Register<ExampleActivity>()
            .Build();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(command.Options.Count, Is.EqualTo(0));
            Assert.That(command.Children.Count(), Is.EqualTo(1));
            Symbol createSymbol = command.Children.First();
            if (createSymbol is Command createCommand)
            {
                Assert.That(createCommand.Children.Count(), Is.EqualTo(CommandFactoryTests.ExpectedChildrenCount));
                Assert.That(createCommand.Options.Count, Is.EqualTo(CommandFactoryTests.ExpectedOptionsCount));
                Assert.That(createCommand.Arguments.Count, Is.EqualTo(CommandFactoryTests.ExpectedArgumentsCount));
            }
            else
                Assert.Fail();
        });
    }
}
