using System.CommandLine;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using StudioLE.CommandLine.Tests.Resources;
using StudioLE.Verify.NUnit;

namespace StudioLE.CommandLine.Tests;

internal sealed class CommandFactoryTests
{
    internal const int ExpectedArgumentsCount = 0;
    internal const int ExpectedOptionsCount = 8;
    internal const int ExpectedChildrenCount = ExpectedArgumentsCount + ExpectedOptionsCount;
    private readonly Verify.Verify _verify = new(new NUnitVerifyContext());

    [Test]
    public async Task CommandFactory_Build()
    {
        // Arrange
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder();
        CommandFactory factory = new(hostBuilder);

        // Act
        Command command = factory.Build(typeof(ExampleActivity));

        // Assert
        await _verify.AsYaml(command
            .Options
            .Select(x => new
            {
                x.Description,
                Type = x.ValueType.Name,
                x.Aliases
            }));
        Assert.Multiple(() =>
        {
            Assert.That(command.Children.Count(), Is.EqualTo(ExpectedChildrenCount));
            Assert.That(command.Options.Count, Is.EqualTo(ExpectedOptionsCount));
            Assert.That(command.Arguments.Count, Is.EqualTo(ExpectedArgumentsCount));
        });
    }
}
