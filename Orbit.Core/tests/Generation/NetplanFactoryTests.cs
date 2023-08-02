using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Generation;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class NetplanFactoryTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly NetplanFactory _factory;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public NetplanFactoryTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _factory = host.Services.GetRequiredService<NetplanFactory>();
        _logs = host.Services.GetTestLogs();
    }

    [Test]
    [Category("Factory")]
    public async Task NetplanFactory_Create()
    {
        // Arrange
        Instance instance = TestHelpers.GetExampleInstance();
        instance.Domains = new[]
        {
            "example.com",
            "example.org"
        };

        // Act
        string output = _factory.Create(instance);

        // Assert
        Assert.That(_logs.Count, Is.EqualTo(0), "Logs Count");
        Assert.That(output, Is.Not.Null);
        await _verify.String(output);
    }
}
