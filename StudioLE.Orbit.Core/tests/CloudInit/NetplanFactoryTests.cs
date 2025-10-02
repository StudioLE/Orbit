using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using StudioLE.Orbit.CloudInit;
using StudioLE.Orbit.Core.Tests.Resources;
using StudioLE.Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace StudioLE.Orbit.Core.Tests.CloudInit;

internal sealed class NetplanFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private NetplanFactory _factory = null!;
    private IReadOnlyCollection<LogEntry> _logs = null!;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
        _factory = host.Services.GetRequiredService<NetplanFactory>();
        _logs = host.Services.GetCachedLogs();
    }

    [Test]
    [Category("Factory")]
    public async Task NetplanFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        instance.Domains = ["example.com", "example.org"];

        // Act
        string output = await _factory.Create(instance);

        // Assert
        Assert.That(_logs.Count, Is.EqualTo(0), "Logs Count");
        Assert.That(output, Is.Not.Null);
        await _context.Verify(output);
    }
}
