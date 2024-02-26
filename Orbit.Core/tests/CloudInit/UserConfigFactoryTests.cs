using System.Runtime.InteropServices;
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

namespace Orbit.Core.Tests.CloudInit;

internal sealed class UserConfigFactoryTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly UserConfigFactory _factory;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public UserConfigFactoryTests()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = TestHelpers.CreateTestHost();
        _factory = host.Services.GetRequiredService<UserConfigFactory>();
        _logs = host.Services.GetCachedLogs();
    }

    [Test]
    [Category("Factory")]
    public async Task CloudInitFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();

        // Act
        string output = _factory.Create(instance);

        // Assert
        Assert.That(_logs.Count, Is.EqualTo(0), "Logs Count");

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(output);
    }
}
