using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.CloudInit;
using Orbit.Core.Tests.Resources;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.CloudInit;

internal sealed class UserConfigActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly UserConfigActivity _activity;
    private readonly UserConfigProvider _provider;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public UserConfigActivityTests()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _activity = provider.GetRequiredService<UserConfigActivity>();
        _provider = provider.GetRequiredService<UserConfigProvider>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task UserConfigActivity_Execute()
    {
        // Arrange
        UserConfigActivity.Inputs inputs = new()
        {
            Instance = new(MockConstants.InstanceName)
        };

        // Act
        UserConfigActivity.Outputs outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(outputs.Status.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        string? retrieved = _provider.Get(inputs.Instance);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved, Is.EqualTo(outputs.Asset?.Content));

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(retrieved!);
    }
}
