using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Orbit.Cli.Utils.CommandLine;
using Orbit.Cli.Utils.Converters;
using Orbit.Core;
using Orbit.Core.Activities;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Cli.Tests;

internal sealed class CommandFactoryTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());

    [Test]
    public async Task CommandFactory_Build()
    {
        // Arrange
        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder()
            .RegisterCustomLoggingProviders()
            .RegisterServices();
        ConverterResolver resolver = ConverterResolver.Default();
        CommandFactory factory = new(hostBuilder, resolver);

        // Act
        Command command = factory.Build(typeof(Create));

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
            Assert.That(command.Children.Count(), Is.EqualTo(21));
            Assert.That(command.Options.Count, Is.EqualTo(21));
            Assert.That(command.Arguments.Count, Is.EqualTo(0));
        });
    }
}
