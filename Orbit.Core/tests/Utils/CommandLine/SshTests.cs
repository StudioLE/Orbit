using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Utils.CommandLine;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Extensions.System;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Utils.CommandLine;

// ReSharper disable once InconsistentNaming
internal sealed class SshTests
{
    private readonly IContext _context = new NUnitContext();
    private Ssh _ssh = null!;
    private IReadOnlyCollection<LogEntry> _logs = null!;

    [SetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");
        IHost host = Host
            .CreateDefaultBuilder()
            .UseTestLoggingProviders()
            .ConfigureServices((builder, services) => services
                .AddTransient<Ssh>()
                .AddOptions<SshOptions>()
                .Bind(builder.Configuration.GetSection(SshOptions.SectionKey)))
            .Build();
        _logs = host.Services.GetCachedLogs();
        _ssh = host.Services.GetRequiredService<Ssh>();
    }

    [Test]
    [Category("Cli")]
    [Explicit("Requires ssh config")]
    public async Task Ssh_Execute_Countdown()
    {
        // Arrange
        // Act
        int exitCode = _ssh.Execute("./countdown", "2 0.5 0");

        // Assert
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
    [Category("Cli")]
    [Explicit("Requires ssh config")]
    public async Task Ssh_Execute_Repeat()
    {
        // Arrange
        const int count = 1000;
        string longString = Enumerable.Range(1, count)
            .Select(i => $"Line {i} of {count}")
            .Join();

        // Act
        int exitCode = _ssh.Execute("./repeat 2 0.2", longString);

        // Assert
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
