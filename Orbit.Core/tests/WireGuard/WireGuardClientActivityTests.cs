using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.WireGuard;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.WireGuard;

internal sealed class WireGuardClientActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private WireGuardClientActivity _activity = null!;
    private WireGuardConfigProvider _provider = null!;
    private IReadOnlyCollection<LogEntry> _logs = null!;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _activity = provider.GetRequiredService<WireGuardClientActivity>();
        _provider = provider.GetRequiredService<WireGuardConfigProvider>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task WireGuardClientActivity_Execute()
    {
        // Arrange
        WireGuardClientActivity.Inputs inputs = new()
        {
            Client = new(MockConstants.ClientName)
        };

        // Act
        WireGuardClientActivity.Outputs? outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(outputs, Is.Not.Null);
        Assert.That(outputs!.Status.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log count");
        Assert.That(outputs.Assets.Count, Is.EqualTo(2), "Output");
        string? resource = await _provider.Get(inputs.Client, $"wg{MockConstants.ServerNumber}.conf");
        Assert.That(resource, Is.Not.Null);
        Assert.That(resource, Is.EqualTo(outputs.Assets.First().Location));

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(resource!);
    }
}
