using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Caddy;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Caddy;

internal sealed class CaddyfileFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private CaddyfileFactory _factory;
    private IReadOnlyCollection<LogEntry> _logs;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
        _factory = host.Services.GetRequiredService<CaddyfileFactory>();
        _logs = host.Services.GetCachedLogs();
    }

    [Test]
    [Category("Factory")]
    public async Task CaddyfileFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        instance.Domains =
        [
            "example.com",
            "example.org"
        ];

        // Act
        string? output = await _factory.Create(instance);

        // Assert
        Assert.That(_logs.Count, Is.EqualTo(0), "Logs Count");
        Assert.That(output, Is.Not.Null);
        await _context.Verify(output!);
    }
}
