using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Creation;
using Orbit.Core.Generation;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class CloudInitFactoryTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly InstanceFactory _instanceFactory;
    private readonly CloudInitFactory _factory;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public CloudInitFactoryTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _factory = host.Services.GetRequiredService<CloudInitFactory>();
        _logs = host.Services.GetTestLogs();
    }

    [Test]
    [Category("Factory")]
    public async Task CloudInitFactory_Create()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(TestHelpers.ExampleInstance());

        // Act
        string output = _factory.Create(instance);

        // Assert
        Assert.That(_logs.Count, Is.EqualTo(0), "Logs Count");
        await _verify.String(output);
    }
}
