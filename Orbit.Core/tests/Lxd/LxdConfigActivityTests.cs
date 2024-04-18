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

namespace Orbit.Core.Tests.Lxd;

internal sealed class LxdConfigActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private LxdConfigActivity _activity;
    private IReadOnlyCollection<LogEntry> _logs;
    private LxdConfigProvider _lxdConfigProvider;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
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
        LxdConfigActivity.Outputs outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(outputs.Status.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Log Count");
        string? resource = await _lxdConfigProvider.Get(inputs.Instance);
        Assert.That(resource, Is.Not.Null);
        Assert.That(resource, Is.EqualTo(outputs.Asset?.Content));

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(resource!);
    }
}
