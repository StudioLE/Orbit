using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using StudioLE.CommandLine.Logging;
using StudioLE.CommandLine.Tests.Resources;
using StudioLE.CommandLine.Utils;
using StudioLE.CommandLine.Utils.Logging.TestLogger;
using StudioLE.Verify.NUnit;

namespace StudioLE.CommandLine.Tests;

internal sealed class ExecutionTests
{
    private readonly StudioLE.Verify.Verify _verify = new(new NUnitVerifyContext());
    private readonly TestLogger _logger = TestLogger.GetInstance();
    private RedirectConsoleToLogger _console = null!;
    private RootCommand _command = null!;

    [SetUp]
    public void SetUp()
    {
        _console = new(_logger);
        IHostBuilder hostBuilder = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ExampleActivity>();
            })
            .RegisterTestLoggingProviders();
        CommandFactory factory = new(hostBuilder);
        _command = new CommandBuilder(factory)
            .Register<ExampleActivity>()
            .Build();
        _command.Name = "RootCommand";
        _logger.Clear();
    }

    [Test]
    public async Task ExecutionTests_No_Command()
    {
        // Arrange
        string[] arguments =
        {
        };

        // Act
        int exitCode = _command.Invoke(arguments, _console);

        // Assert
        _console.Flush();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(1), "Exit code");
            Assert.That(_logger.Logs.Count(x => x.LogLevel == LogLevel.Error), Is.EqualTo(2), "Error count");
        });
        await _verify.AsString(_logger);
    }

    [Test]
    public void ExecutionTests_Command_LowerCase()
    {
        // Arrange
        string[] arguments =
        {
            "exampleactivity"
        };

        // Act
        int exitCode = _command.Invoke(arguments, _console);

        // Assert
        _console.Flush();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(0), "Exit code");
            Assert.That(_logger.Logs.Count(x => x.LogLevel == LogLevel.Error), Is.EqualTo(0), "Error count");
        });
    }

    [Test]
    public void ExecutionTests_Command_CamelCase()
    {
        // Arrange
        string[] arguments =
        {
            "ExampleActivity"
        };

        // Act
        int exitCode = _command.Invoke(arguments, _console);

        // Assert
        _console.Flush();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(1), "Exit code");
            Assert.That(_logger.Logs.Count(x => x.LogLevel == LogLevel.Error), Is.EqualTo(3), "Error count");
        });
    }

    [Test]
    public async Task ExecutionTests_Invalid_Argument()
    {
        // Arrange
        string[] arguments =
        {
            "exampleactivity",
            "--nope",
            "1"
        };

        // Act
        int exitCode = _command.Invoke(arguments, _console);

        // Assert
        _console.Flush();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(1), "Exit code");
            Assert.That(_logger.Logs.Count(x => x.LogLevel == LogLevel.Error), Is.EqualTo(3), "Error count");
        });
        await _verify.AsString(_logger);
    }

    [Test]
    public async Task ExecutionTests_Validation()
    {
        // Arrange
        string[] arguments =
        {
            "exampleactivity",
            "--number",
            "1"
        };

        // Act
        int exitCode = _command.Invoke(arguments, _console);

        // Assert
        _console.Flush();
        Assert.Multiple(() =>
        {
            Assert.That(exitCode, Is.EqualTo(0), "Exit code");
            Assert.That(_logger.Logs.Count(x => x.LogLevel == LogLevel.Error), Is.EqualTo(0), "Error count");
        });
        await _verify.AsString(_logger);
    }
}
