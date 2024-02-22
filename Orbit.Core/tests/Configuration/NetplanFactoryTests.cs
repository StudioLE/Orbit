using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.CloudInit;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Configuration;

internal sealed class NetplanFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly NetplanFactory _factory;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public NetplanFactoryTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _factory = host.Services.GetRequiredService<NetplanFactory>();
        _logs = host.Services.GetCachedLogs();
    }

    [Test]
    [Category("Factory")]
    public async Task NetplanFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        TestHelpers.UseMockMacAddress(instance);
        instance.Domains =
        [
            "example.com",
            "example.org"
        ];

        // Act
        string output = _factory.Create(instance);

        // Assert
        Assert.That(_logs.Count, Is.EqualTo(0), "Logs Count");
        Assert.That(output, Is.Not.Null);
        await _context.Verify(output);
    }
}
