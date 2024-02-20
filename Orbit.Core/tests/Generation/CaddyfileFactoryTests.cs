using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Caddy;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class CaddyfileFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CaddyfileFactory _factory;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public CaddyfileFactoryTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _factory = host.Services.GetRequiredService<CaddyfileFactory>();
        _logs = host.Services.GetCachedLogs();
    }

    [Test]
    [Category("Factory")]
    public async Task CaddyfileFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        instance.Domains = new[]
        {
            "example.com",
            "example.org"
        };

        // Act
        string? output = _factory.Create(instance);

        // Assert
        Assert.That(_logs.Count, Is.EqualTo(0), "Logs Count");
        Assert.That(output, Is.Not.Null);
        await _context.Verify(output!);
    }
}
