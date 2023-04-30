using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
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
        IHost host = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services => services
                .AddCommandBuilderServices()
                .AddTransient<ExampleActivity>())
            .Build();
        CommandBuilder builder = host
            .Services
            .GetRequiredService<CommandBuilder>();

        // Act
        RootCommand command = builder
            .Register<ExampleActivity>()
            .Build();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(command.Options.Count, Is.EqualTo(0));
            Assert.That(command.Children.Count(), Is.EqualTo(1));
            Symbol createSymbol = command.Children.First();
            if (createSymbol is Command subCommand)
            {
                Assert.That(subCommand.Children.Count(), Is.EqualTo(CommandFactoryTests.ExpectedChildrenCount));
                Assert.That(subCommand.Options.Count, Is.EqualTo(CommandFactoryTests.ExpectedOptionsCount));
                Assert.That(subCommand.Arguments.Count, Is.EqualTo(CommandFactoryTests.ExpectedArgumentsCount));
            }
            else
                Assert.Fail();
        });
    }
}
