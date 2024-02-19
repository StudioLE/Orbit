using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Utils.Shell;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Extensions.System;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Utils.Shell;

// ReSharper disable once InconsistentNaming
internal sealed class ShellCommandTests
{
    private readonly IContext _context = new NUnitContext();
    private ShellCommand _cmd = null!;
    private IReadOnlyCollection<LogEntry> _logs = null!;

    [SetUp]
    public void SetUp()
    {
        IHost host = Host
            .CreateDefaultBuilder()
            .UseTestLoggingProviders()
            .ConfigureServices(services => services
                .AddTransient<ShellCommand>())
            .Build();
        _logs = host.Services.GetCachedLogs();
        _cmd = host.Services.GetRequiredService<ShellCommand>();
    }

    [Test]
    [Category("Misc")]
    public async Task ShellCommand_Execute_Countdown()
    {
        // Arrange
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "countdown");
        if (!Path.Exists(path))
            throw new("The countdown executable is missing.");

        // Act
        int exitCode = _cmd.Execute(path, "2 0.5 0");

        // Assert
        if (_logs.Any(log => log is
            {
                LogLevel: LogLevel.Error
            }))
        {
            Assert.Inconclusive();
            return;
        }

        Assert.That(exitCode, Is.EqualTo(0), "Exit code");
        Assert.That(_logs.Any(log => log.LogLevel == LogLevel.Error), Is.False, "Error logs");
        Assert.That(_logs.Any(log => log.LogLevel == LogLevel.Warning), Is.False, "Warning logs");
        string output = _logs
            .Where(log => log.LogLevel == LogLevel.Information)
            .Select(log => $"[{log.Category}]{Environment.NewLine}{log.Message}")
            .Join();
        await _context.Verify(output);
    }

    [Test]
    [Category("Misc")]
    public async Task ShellCommand_Execute_Repeat()
    {
        // Arrange
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "repeat");
        if (!Path.Exists(path))
            throw new("The countdown executable is missing.");
        const int count = 1000;
        string longString = Enumerable.Range(1, count)
            .Select(i => $"Line {i} of {count}")
            .Join();

        // Act
        int exitCode = _cmd.Execute(path, "2 0.2", longString);

        // Assert
        if (_logs.Any(log => log is
            {
                LogLevel: LogLevel.Error
            }))
        {
            Assert.Inconclusive();
            return;
        }

        Assert.That(exitCode, Is.EqualTo(0), "Exit code");
        Assert.That(_logs.Any(log => log.LogLevel == LogLevel.Error), Is.False, "Error logs");
        Assert.That(_logs.Any(log => log.LogLevel == LogLevel.Warning), Is.False, "Warning logs");
        string output = _logs
            .Where(log => log.LogLevel == LogLevel.Information)
            .Select(log => $"[{log.Category}]{Environment.NewLine}{log.Message}")
            .Join();
        await _context.Verify(output);
    }
}
