using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Lxd;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;
using Tectonic;

namespace Orbit.Core.Tests.Lxd;

internal sealed class LxdConfigActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CommandContext _commandContext;
    private readonly LxdConfigActivity _activity;
    private readonly IReadOnlyCollection<LogEntry> _logs;
    private readonly LxdConfigProvider _lxdConfigProvider;

    public LxdConfigActivityTests()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _commandContext = provider.GetRequiredService<CommandContext>();
        _activity = provider.GetRequiredService<LxdConfigActivity>();
        _lxdConfigProvider = provider.GetRequiredService<LxdConfigProvider>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task LxdConfigActivity_Execute()
    {
        // Arrange
        LxdConfigActivity.Inputs inputs = new()
        {
            Instance = new(MockConstants.InstanceName)
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        Assert.That(output, Is.Empty, "Output");
        string? resource = _lxdConfigProvider.Get(inputs.Instance);
        Assert.That(resource, Is.Not.Null);

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(resource!);
    }
}
